
using UnityEngine;
using System.Collections.Generic;

public class HouseManager : MonoBehaviour {
	#region Public Variables
	[Tooltip("The allowed pool of room styles that will be chosen from.")]
	public List<GameObject> DefaultRoomStyles;
	[Tooltip("Game object to parent all rooms to")]
	public GameObject House;
	[Tooltip("How many rooms the house is wide")]
	public int HouseWidth;
	[Tooltip("How many rooms the house is long")]
	public int HouseLength;
	public bool RandomizeRooms;
	[Tooltip("Size of the rooms to be placed.")]
	public float RoomSize;
	[Tooltip("Options for generating floor patterns\n" +
		"0: Random Generation\n" +
		"1: Simple Unique Connection\n" +
		"2: Reverse Path Creation\n" +
		"3: Nodal Branching.")]
	public int FloorGenSetting;
	[Tooltip("The number of floors for the house to have.")]
	public int NumberOfFloors;
	[Tooltip("Prefab to set up floor heiarchy.")]
	public GameObject Floor;


	[Header("Debug Settings")]
	[Tooltip("Whether or not to display the room numbers and grid.")]
	public bool DebugOverlay;
	public GameObject DebugOverlaySquare;
	#endregion

	#region Private Variables
	GameObject house;
	GameObject[] floors;
	FloorController[] floorControllers;

	List<GameObject> uniqueRooms;
	Scenario scenario;
	#endregion

	public void Initialize(Scenario inScenario){
		// Save a copy of the scenario
		scenario = inScenario;

		// Override allowed rooms and unique rooms with 
		// the scenario specific ones, if any.
		if (scenario.AllowedFillerRooms.Count > 0) {
			DefaultRoomStyles = scenario.AllowedFillerRooms;
		}
		uniqueRooms = new List<GameObject>(scenario.UniqueRooms);
	

		// Create the house
		house = new GameObject();
		house.name = "House";
		floors = new GameObject[NumberOfFloors];
		floorControllers = new FloorController[NumberOfFloors];
		// Initialize the floors
		for(var i = 0; i < NumberOfFloors; i++){
			GameObject floor = Instantiate(Floor);
			floor.transform.parent = house.transform;
			floor.name = string.Format("Floor{0}",i);
			FloorController floorControl = floor.GetComponent<FloorController>();
			floorControl.Initialize(HouseWidth,HouseLength,RoomSize,allowedRoomStyles: DefaultRoomStyles);
			floor.transform.position = new Vector3(0,i*floorControl.FloorHeight,HouseLength*RoomSize/2f);
			floors[i] = floor;
			floorControllers[i] = floorControl;
		}

		SetUpFloors();
	}

	/// <summary>
	/// This method is a workspace to set the floors to user specifications.
	/// This is in no way generic.
	/// </summary>
	void SetUpFloors(){

		// Set the center first 3 rooms to be a particular set up.
		int centerRow = HouseWidth/2;
		floorControllers[0].SetRoom(new int[]{centerRow,0},new int[]{1,1,0,1},"Debug");
		floorControllers[0].SetRoom(new int[]{centerRow,1},new int[]{1,1,1,1},"Debug");
		floorControllers[0].SetRoom(new int[]{centerRow,2},new int[]{0,0,1,0},"Debug");

		// Generate all the floors based on the 
		// branching techniques.
		for(var i = 0; i < NumberOfFloors; i++){
			floorControllers[i].GenerateFloor(3);
			floorControllers[i].SetLayer(LayerMask.NameToLayer(string.Format("Floor{0}",i)));
		}
	}

}
