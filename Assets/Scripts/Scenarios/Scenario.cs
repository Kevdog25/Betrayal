using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[System.Serializable]
public class Scenario {
	public delegate bool EvaluateComponent(ScenarioComponent comp);

	public string Name;
	public bool OverwriteLock;
	public int HouseWidth;
	public int HouseLength;
	public float RoomSize;
	public List<GameObject> AllowedFillerRooms;
	public List<GameObject> UniqueRooms;
	public Room[][,] FixedRooms;
	public Dictionary<string,List<ScenarioComponent>> Components; 

	static string saveFolder = Path.Combine(Application.dataPath,"../Scenarios/");

    int timeBetweenSegments = 300;
    List<PlayerController> BadGuys;
    List<PlayerController> GoodGuys;
    [System.NonSerialized]
    GameController ruleController;

	public Scenario(){
		AllowedFillerRooms = new List<GameObject>();
		UniqueRooms = new List<GameObject>();

		Components = new Dictionary<string, List<ScenarioComponent>>();
		Components.Add("Bad Guy",new List<ScenarioComponent>());
		Components.Add("Bad Guys",new List<ScenarioComponent>());
		Components.Add("Good Guy",new List<ScenarioComponent>());
		Components.Add("Good Guys",new List<ScenarioComponent>());
        Components.Add("Rules", new List<ScenarioComponent>());

        BadGuys = new List<PlayerController>();
        GoodGuys = new List<PlayerController>();
	}

    /// <summary>
    /// Registers a bad guy to be affected by the 
    /// bad guy specific components
    /// </summary>
    /// <param name="player">player controller to register</param>
    public void RegisterBadGuy(PlayerController player)
    {
        BadGuys.Add(player);
    }

    /// <summary>
    /// Registers a good guy to be affected by the 
    /// good guy specific components
    /// </summary>
    /// <param name="player">player controller to register</param>
    public void RegisterGoodGuy(PlayerController player)
    {
        GoodGuys.Add(player);
    }

    /// <summary>
    /// Sets the object to have rule affecting components attached
    /// to it. Warning gaurds against overwriting existing rules.
    /// </summary>
    /// <param name="controller">game controller to register</param>
    public void RegisterGameController(GameController controller)
    {
        if(ruleController == null
            || ruleController.GetComponent<ScenarioComponent>() == null)
            ruleController = controller;
        else
        {
            Debug.LogWarning("Trying to overwrite a rule controller" +
                             "with rules in effect.");
        }
    }

    public void Start()
    {

    }

    /// <summary>
    /// Controls the timing of scenario events.
    /// </summary>
    /// <returns>enumerator to delay execution.</returns>
    IEnumerator Timings()
    {
        yield return new WaitForSeconds(timeBetweenSegments);
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
		if(File.Exists(saveFolder + name)){
			using(var stream = new FileStream(saveFolder + name,FileMode.Open)){
                try
                {
                    return binaryFormatter.Deserialize(stream) as Scenario;
                }
                catch(EndOfStreamException ex)
                {
                    throw new EndOfStreamException("Failed to load file: " + name,ex);
                }
			}
		}

		return null;
	}
}
