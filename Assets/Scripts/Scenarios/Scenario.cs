using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[System.Serializable]
public class Scenario {

	[SerializeField]
	public string Name;
	[SerializeField]
	public List<GameObject> AllowedFillerRooms;
	public List<GameObject> UniqueRooms;
	public Room[][,] FixedRooms;


	public Scenario(){
		AllowedFillerRooms = new List<GameObject>();
		UniqueRooms = new List<GameObject>();

	}

	/// <summary>
	/// Save this object to the specified xml file.
	/// </summary>
	/// <param name="path">Path.</param>
	public void Save(string path){
		var binaryFormatter = new BinaryFormatter();
		using(var stream = new FileStream(path,FileMode.Create)){
			Debug.Log("Saving Scenario " + Name);
			binaryFormatter.Serialize(stream,this);
		}
	}

	/// <summary>
	/// Load object from the specified xml file.
	/// </summary>
	/// <param name="path">Path.</param>
	public static Scenario Load(string path){
		var binaryFormatter = new BinaryFormatter();
		using(var stream = new FileStream(path,FileMode.Open)){
			return binaryFormatter.Deserialize(stream) as Scenario;
		}
	}
}
