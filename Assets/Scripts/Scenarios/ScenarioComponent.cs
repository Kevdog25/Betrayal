using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

/// <summary>
/// This interface is used to display generic component properties for
/// modification through the Scenario Editor.
/// </summary>
[System.Serializable]
public abstract class ScenarioComponent{

	public string Name;
	public bool OverwriteLock = true;

	static string saveFolder = Path.Combine(Application.dataPath,"../ScenarioComponents/");


	protected Dictionary<string,string> StringValues = new Dictionary<string, string>();
	protected Dictionary<string,int> IntValues = new Dictionary<string, int>();
	protected Dictionary<string,float> FloatValues = new Dictionary<string, float>();
	protected string[] Tags;

	public ScenarioComponent(){
	}

	
	/// <summary>
	/// Save this object to the specified binary file.
	/// </summary>
	/// <param name="path">Path.</param>
	public void Save(){
		var binaryFormatter = new BinaryFormatter();
		using(var stream = new FileStream(saveFolder + Name + ".txt",FileMode.Create)){
			binaryFormatter.Serialize(stream,this);
		}
	}
	
	/// <summary>
	/// Load object from the specified binary file.
	/// </summary>
	/// <param name="name">Name of the component.</param>
	public static ScenarioComponent Load(string name){
		var binaryFormatter = new BinaryFormatter();
		if(File.Exists(saveFolder + name)){
			using(var stream = new FileStream(saveFolder + name,FileMode.Open)){
				return binaryFormatter.Deserialize(stream) as ScenarioComponent;
			}
		}

		return null;
	}

	public abstract string[] GetTags();

	/// <summary>
	/// Returns a dictionary of name : value pairs 
	/// that the scenario component uses.
	/// </summary>
	/// <returns>The string valued properties.</returns>
	public Dictionary<string,string> GetStringProperties(){
		return StringValues;
	}
	
	/// <summary>
	/// Returns a dictionary of name : value pairs 
	/// that the scenario component uses.
	/// </summary>
	/// <returns>The int valued properties.</returns>
	public Dictionary<string,int> GetIntProperties(){
		return IntValues;
	}

	/// <summary>
	/// Returns a dictionary of name : value pairs 
	/// that the scenario component uses.
	/// </summary>
	/// <returns>The float valued properties.</returns>
	public Dictionary<string,float> GetFloatProperties(){
		return FloatValues;
	}

	/// <summary>
	/// Sets the value of a string parameter.
	/// Enforces any component specific constraint
	/// </summary>
	/// <param name="key">Key.</param>
	/// <param name="value">Value.</param>
	public virtual void SetString(string key, string value){
		StringValues[key] = value;
	}
	
	/// <summary>
	/// Sets the value of an int parameter.
	/// Enforces any component specific constraint
	/// </summary>
	/// <param name="key">Key.</param>
	/// <param name="value">Value.</param>
	public virtual void SetInt(string key, int value){
		IntValues[key] = value;
	}
	
	/// <summary>
	/// Sets the value of a float parameter.
	/// Enforces any component specific constraint
	/// </summary>
	/// <param name="key">Key.</param>
	/// <param name="value">Value.</param>
	public virtual void SetFloat(string key, float value){
		FloatValues[key] = value;
	}

	public bool Equals(ScenarioComponent you){

		return 
			(Name == you.Name &&
			 StringValues.Equals(you.GetStringProperties()) &&
			 IntValues.Equals(you.GetIntProperties()) &&
			 FloatValues.Equals(you.GetFloatProperties()));
	}
}
