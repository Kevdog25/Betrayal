using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[System.Serializable]
public class Scenario {

	public string Name;
	public int HouseWidth;
	public int HouseLength;
	public float RoomSize;
	public List<GameObject> AllowedFillerRooms;
	public List<GameObject> UniqueRooms;
	public Room[][,] FixedRooms;
	public Dictionary<string,List<ScenarioComponent>> Components; 

	static string saveFolder = Path.Combine(Application.dataPath,"../Scenarios/");

	public Scenario(){
		AllowedFillerRooms = new List<GameObject>();
		UniqueRooms = new List<GameObject>();

		Components = new Dictionary<string, List<ScenarioComponent>>();
		Components.Add("Bad Guy",new List<ScenarioComponent>());
		Components.Add("Bad Guys",new List<ScenarioComponent>());
		Components.Add("Good Guy",new List<ScenarioComponent>());
		Components.Add("Good Guys",new List<ScenarioComponent>());
	}

	/// <summary>
	/// Save this object to the specified binary file.
	/// </summary>
	/// <param name="path">Path.</param>
	public void Save(){
		var binaryFormatter = new BinaryFormatter();
		using(var stream = new FileStream(saveFolder + Name + ".txt",FileMode.Create)){
			Debug.Log("Saving Scenario " + Name);
			binaryFormatter.Serialize(stream,this);
		}
	}

	/// <summary>
	/// Load object from the specified binary file.
	/// </summary>
	/// <param name="name">Name of the scenario.</param>
	public static Scenario Load(string name){
		var binaryFormatter = new BinaryFormatter();
		using(var stream = new FileStream(saveFolder + name,FileMode.Open)){
			return binaryFormatter.Deserialize(stream) as Scenario;
		}
	}
}
