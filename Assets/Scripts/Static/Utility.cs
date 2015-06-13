using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Utility{

	#region Random Element Selectors
	/// <summary>
	/// Selects a random element from the list.
	/// </summary>
	/// <returns>The element.</returns>
	/// <param name="list">List.</param>
	public static T RandomElement<T>(T[] list)
	{
		if (list.Length == 0){
			Debug.LogError ("Cannot find element in empty list");
			return default(T);
		}

		return list [Random.Range (0, list.Length)];
	}
	/// <summary>
	/// Selects a random element from the list.
	/// </summary>
	/// <returns>The element.</returns>
	/// <param name="list">List.</param>
	public static T RandomElement<T>(IList<T> list)
	{
		if (list.Count == 0){
			Debug.LogError ("Cannot find element in empty list");
			return default(T);
		}
		
		return list [Random.Range (0, list.Count)];
	}

	/// <summary>
	/// Gets a list of random elements that are a subset of the 
	/// provided list.
	/// </summary>
	/// <returns>The elements.</returns>
	/// <param name="list">List.</param>
	/// <param name="nElements">N elements.</param>
	/// <param name="repeats">If set to <c>true</c> repeats.</param>
	public static T[] RandomElements<T>(T[] list,int nElements,bool repeats = false){
		
		if ((!repeats) && (nElements > list.Length) || (list.Length == 0)) {
			Debug.LogError(string.Format(
				"Insufficient element pool: Have {0}. Asked for {1}",
				list.Length,
				nElements));
			return null;
		}
		
		var rndElements = new T[nElements];
		if (repeats) {
			for (int i=0; i<nElements; i++) {
				rndElements [i] = list[Random.Range(0,list.Length)];
			}
		} else {
			var remainingElements = new List<T>(list);
			for (int i = 0; i < nElements;i++){
				rndElements[i] = list[Random.Range(0,list.Length)];
				remainingElements.Remove(rndElements[i]);
			}
		}
		
		return rndElements;
	}
	#endregion

	#region String Formatting
	/// <summary>
	/// Tos the string.
	/// </summary>
	/// <returns>The string that was to'd.</returns>
	/// <param name="array">Array to, to the string.</param>
	/// <param name="separator">Separator.</param>
	/// <typeparam name="T">The type of data in the array.</typeparam>
	public static string ToString<T>(T[] array,string separator = ", "){
		string output = "["+array[0].ToString();
		for(var i = 1; i < array.Length; i++){
			output += separator + array [i];
		}
		return output+"]";
	}

	/// <summary>
	/// Tos the string.
	/// </summary>
	/// <returns>The string.</returns>
	/// <param name="separator">Separator.</param>
	/// <param name="array">Array.</param>
	public static string ToString(IList<int[]> array,string separator = ", "){
		string output = "["+ToString(array[0],separator);
		for(var i = 1; i < array.Count; i++){
			output += " " + ToString(array[i],separator);
		}
		return output+"]";
	}
	#endregion

	#region Sampling From Distributions
	/// <summary>
	/// Samples from a Gaussian distribution.
	/// This method is slow and sucks. Use sparingly.
	/// </summary>
	/// <returns>The sampled value.</returns>
	/// <param name="mean">Mean.</param>
	/// <param name="std">Std.</param>
	public static float SampleFromGaussian(float mean, float std){
		bool foundIt = false;
		float r = 0;

		// Loop until found a suitable value
		while(!foundIt){
			r = Random.Range(-2*std,2*std);
			float gauss = Mathf.Exp(-(r*r)/(2*std*std));
			if(Random.value < gauss){
				foundIt = true;
			}
		}

		// Return the shifted value.
		return r + mean;
	}
	#endregion

}
