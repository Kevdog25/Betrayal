
using UnityEngine;
using System.Collections.Generic;

public class HouseManager : MonoBehaviour {
	#region Public Variables
	[Tooltip("The allowed pool of room styles that will be chosen from.")]
	public GameObject[] DefaultRoomStyles;
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
	[Tooltip("The particle effects to shroud undiscovered rooms.")]
	public GameObject ShroudParticles;
	[Tooltip("How large the shroud covers are compared to the rooms.")]
	public float ShroudSizeRatio;
	[Header("Debug Settings")]
	[Tooltip("Whether or not to display the room numbers and grid.")]
	public bool DebugOverlay;
	public GameObject DebugOverlaySquare;
	[Tooltip("A room style used as a maker when debugging the layout.")]
	public GameObject DebugRoomStyle;
	[Tooltip("Should the rooms be covered in smoke?")]
	public bool ShroudOn;
	[Tooltip("Should all rooms be visible on start?")]
	public bool VisibleOnStart;
	#endregion

	#region Private Variables
	GameObject GroundRooms;
	GameObject BasementRooms;
	GameObject UpstairsRooms;
	GameObject house;
	GameObject shroud;
	
	Room[,] Ground;
	Room[,] Basement;
	Room[,] Upstairs;
	PathCreator pathMaker;

	List<GameObject> uniqueRooms;
	Scenario scenario;
	#endregion

	public void Initialize(Scenario inScenario){
		// Save a copy of the scenario
		scenario = inScenario;
		pathMaker = GetComponent<PathCreator> ();

		// Override allowed rooms and unique rooms with 
		// the scenario specific ones, if any.
		if (scenario.AllowedFillerRooms.Length > 0) {
			DefaultRoomStyles = scenario.AllowedFillerRooms;
		}
		uniqueRooms = new List<GameObject>(scenario.UniqueRooms);
	

		SetUpRoomHeirarchy ();

		// Initialize and set up the Ground Floor
		Ground = new Room[HouseWidth,HouseLength];
		InitializeFloor (Ground);
		GenerateFloor (Ground,GroundRooms);
	}

	/// <summary>
	/// Sets the appropriate parents and transforms for organization.
	/// </summary>
	void SetUpRoomHeirarchy(){
		house = new GameObject();
		house.name = "House";
		var roomContainerClone = Instantiate (house);
		roomContainerClone.transform.parent = transform;
		roomContainerClone.name = "House";
		GroundRooms = new GameObject ();
		GroundRooms.name = "GroundFloor";
		BasementRooms = new GameObject ();
		BasementRooms.name = "BasementFloor";
		UpstairsRooms = new GameObject ();
		UpstairsRooms.name = "UpstairsFloor";

		shroud = new GameObject();
		shroud.name = "Shroud";

		GroundRooms.transform.parent = roomContainerClone.transform;
		BasementRooms.transform.parent = roomContainerClone.transform;
		UpstairsRooms.transform.parent = roomContainerClone.transform;
	}

	/// <summary>
	/// Generates the floor plan.
	/// </summary>
	/// <param name="floor">Floor.</param>
	/// <param name="parent">The gameobject to parent all the rooms to.</param>
	void GenerateFloor(Room[,] floor,GameObject parent){
		// Select the generation setting
		switch(FloorGenSetting){
		case 0:
			pathMaker.RandomizeFloor(floor);
			break;
		case 1:
			ConnectUniqueRooms(floor);
			break;
		case 2:
			ReversePathFinding(floor);
			break;
		case 3:
			BranchFromCenter(floor);
			break;
		}

		// Now that the floors have been generated
		// Set the rooms
		for (int i=0; i<HouseWidth; i++) {
			for (int j=0; j<HouseLength; j++) {
				// Cover the room with a shroud of smoke initially
				GameObject shroudClone = Instantiate(ShroudParticles) as GameObject;
				shroudClone.transform.position += floor[i,j].Position;
				shroudClone.transform.localScale = new Vector3(RoomSize*ShroudSizeRatio,
				                                               RoomSize*ShroudSizeRatio,
				                                               1);
				shroudClone.transform.parent = shroud.transform;
				// If the ShroudOn setting is enabled
				if(ShroudOn)
					shroudClone.GetComponent<ParticleSystem>().Play();

				if(floor[i,j].Set){
					
					floor[i,j].Position = 
						new Vector3((i+0.5f)*RoomSize,0,(j+0.5f)*RoomSize)
							-new Vector3(RoomSize * HouseWidth/2f, 0, RoomSize*HouseLength/2f);

					// Place the room in real space.
					GameObject room = PlaceRoom(floor[i,j]);
					// Set its parent to the parent, for categorization.
					room.transform.parent = parent.transform;

					floor[i,j].RoomObject.GetComponent<RoomController>().Shroud = shroudClone;
				}
			}
		}

		// Set up the doors
		PlaceDoors(floor);
	}

	/// <summary>
	/// Places the doors for the floor.
	/// This is done based on the <see cref="Room.GetDoors()"/> method.
	/// </summary>
	/// <param name="floor">Floor.</param>
	void PlaceDoors(Room[,] floor){
		for(var i = 0; i < floor.GetLength(0); i++){
			for(var j = 0; j < floor.GetLength(1); j++){
				Room room = floor[i,j];
				if(room.Set){
					int[] doors = room.GetDoors();
					RoomController roomControl = room.RoomObject.GetComponent<RoomController>();

					for(var k = 0; k < doors.Length/2;k++){
						if(doors[k] == 1){
							GameObject doorClone = Instantiate(roomControl.Door);
							Transform doorTrans = doorClone.transform;
							EasyScaling scaling = doorClone.GetComponent<EasyScaling>();
							//Scale it in the x direction to fit the doorway.
							scaling.Scale(RoomSize/roomControl.DefaultRoomSize,1,1);
							doorTrans.position = new Vector3(1.25f*RoomSize/roomControl.DefaultRoomSize,
							                                      0,
							                                      RoomSize/2f) +
												room.RoomObject.transform.position;

							// Rotate it around the center.
							doorTrans.RotateAround(room.RoomObject.transform.position,
							                       Vector3.up,
							                       (k*360f/doors.Length));
							DoorController doorControl = doorClone.GetComponent<DoorController>();
							if(k == 0){
								var otherRoomControl = floor[i,j+1].RoomObject.GetComponent<RoomController>();

								// Set up event listeners to activate on room reveal, and door open.
								roomControl.ListenForOpen(doorControl);
								otherRoomControl.ListenForOpen(doorControl);
								doorControl.ListenToReveal(roomControl);
								doorControl.ListenToReveal(otherRoomControl);
							}
							if(k == 1){
								var otherRoomControl = floor[i+1,j].RoomObject.GetComponent<RoomController>();
								
								// Set up event listeners to activate on room reveal, and door open.
								roomControl.ListenForOpen(doorControl);
								otherRoomControl.ListenForOpen(doorControl);
								doorControl.ListenToReveal(roomControl);
								doorControl.ListenToReveal(otherRoomControl);
							}
							if(room.RoomType == "Debug"){
								// If the room type is debug, then go ahead and reveal it.
								roomControl.Reveal();
							}
						}
					}
				}
			}
		}

	}

	/// <summary>
	/// Builds the gameobject attached to Room.
	/// </summary>
	/// <param name="room">Room.</param>
	GameObject PlaceRoom(Room room){

		// Choose a roomstyle based on the decided room placement
		GameObject roomStyle;
		switch (room.RoomType) {
		case "Random":
			roomStyle = Utility.RandomElement (DefaultRoomStyles);
			break;
		case "Unique":
			roomStyle = (Utility.RandomElement (uniqueRooms));
			uniqueRooms.Remove (roomStyle);
			break;
		case "Debug":
			roomStyle = DebugRoomStyle;
			break;
		default:
			Debug.Log ("PlaceRoom error. Specified RoomType is not recognized.");
			roomStyle = Utility.RandomElement (DefaultRoomStyles);
			break;
		}

		// room is the Struct, and roomClone is the gameObject
		// with roomControl as the roomClone's RoomController instance

		// Instantiate the room object
		room.RoomObject = Instantiate(roomStyle);
		var roomClone = room.RoomObject;
		// Get the room controller script
		RoomController roomControl = 
			roomClone.GetComponent<RoomController>();
		// Designate the position of the room
		roomControl.Size = RoomSize;
		roomClone.transform.localPosition = room.Position;

		// Set Active to true (WIP)
		roomClone.gameObject.SetActive(true);

		roomControl.SetUp(room.GetDoors());

		// Deactivate any rooms that should be hidden.
		if(room.RoomType != "Debug" && !VisibleOnStart){
			roomClone.gameObject.SetActive(false);
		}

		// Return a reference to the created room
		return roomClone;
	}

	#region Floor Generation Methods
	/// <summary>
	/// Connects all uniqueRooms. This must be called before
	/// any rooms are placed to ensure that uniqueRooms is full.
	/// </summary>
	/// <param name="floor">Floor to generate.</param>
	void ConnectUniqueRooms(Room[,] floor){
		// Generate the positions of each unique room.
		List<int[]> uniquePositions = SetUniqueRooms(floor,uniqueRooms.Count);

		// Loop Through and connect them.
		for(var i = 0; i < uniquePositions.Count - 1; i++){
			pathMaker.Meander(uniquePositions[i],uniquePositions[i+1],floor);
		}
	}

	/// <summary>
	/// Generates a floor plan by inhibiting path creation into the corner.
	/// Ignores <see cref="pathMaker.MeanderRate"/>.
	/// </summary>
	/// <param name="floor">Floor to generate.</param>
	void ReversePathFinding(Room[,] floor){
		// Meander away from the corner
		// Fix the meander rate to inhibit motion 
		float meanderRate = pathMaker.MeanderRate;
		pathMaker.MeanderRate = 0.1f;
		pathMaker.Meander(new int[]{0,0},new int[]{-1,-1},floor);
		pathMaker.MeanderRate = meanderRate;
	}

	/// <summary>
	/// Branches from a node in the center of the floor
	/// </summary>
	/// <param name="floor">Floor.</param>
	void BranchFromCenter(Room[,] floor){
		// Choose the positions of the unique rooms
		List<int[]> uniquePositions = SetUniqueRooms(floor,uniqueRooms.Count);

		// Start the node at the center
		var startNode = new int[]{HouseWidth/2,HouseLength/2};
		// For debugging
		floor[startNode[0],startNode[1]].RoomType = "Debug";

		pathMaker.BranchNode(startNode,floor);

		// Check to make sure that each important room is set.
		for(var i = 0; i < uniquePositions.Count;i++){
			int[] pos = uniquePositions[i];

			// If it isnt, then meander to it.
			if(!floor[pos[0],pos[1]].Set){
				pathMaker.Meander(startNode,pos,floor);
			}
		}
	}
	#endregion

	#region Private Methods
	/// <summary>
	/// Sets the unique rooms. This attempts to optimally set the distance
	/// between rooms.
	/// </summary>
	/// <returns>The location of the unique rooms.</returns>
	/// <param name="dimensions">Dimensions of the floor: {width,height}.</param>
	/// <param name="nRooms">Number of rooms.</param>
	List<int[]> SetUniqueRooms(Room[,] floor,int nRooms,
	                           int[] centroid = null){

		// If no centroid for room placement is specified, use the center of the floor.
		if(centroid == null){
			centroid = new int[]{floor.GetLength(0)/2,floor.GetLength(1)/2};
		}
		var list = new List<int[]> ();

		// Start the average position at the centroid given.
		int[] avgUniquePosition = new int[]{centroid[0],centroid[1]};

		// Try to get the positions of each room randomly.
		while(list.Count < nRooms){

			int[] xy;
			// Try to pick a position that is on the board and is distributed by a gaussian.
			do{
				float r = Utility.SampleFromGaussian(7,2);
				float theta = Random.Range(0,2*Mathf.PI);
				xy = new int[]{(int)(r*Mathf.Cos(theta)),(int)(r*Mathf.Sin(theta))};
				xy[0] += avgUniquePosition[0];
				xy[1] += avgUniquePosition[1];
			}
			while((xy[0] < 0) || (xy[1] < 0) ||
			      (xy[0] >= floor.GetLength(0)) || (xy[1] >= floor.GetLength(1)));

			// If it picked an unused position, then add it to the list.
			if(!list.Contains(xy)){
				list.Add(xy);
				floor[xy[0],xy[1]].RoomType = "Unique";

				// Get the average position of existing unique rooms.
				float n = list.Count;
				avgUniquePosition[0] = (int)(centroid[0]/(n+1));
				avgUniquePosition[1] = (int)(centroid[1]/(n+1));
				for(var i = 0; i < n; i++){
					avgUniquePosition[0] += (int)(list[i][0]/(n+1));
					avgUniquePosition[1] += (int)(list[i][1]/(n+1));
				}
			}
		}

		return list;
	}

	/// <summary>
	/// Initializes the floor. Sets the local positioning of all of the rooms.
	/// </summary>
	/// <param name="floor">Floor to initialize.</param>
	void InitializeFloor(Room[,] floor){
		for(int i = 0; i < HouseWidth;i++){
			for(int j = 0; j < HouseLength;j++){
				floor[i,j] = new Room();
				floor[i,j].Position = 
					new Vector3((i+0.5f)*RoomSize,0,(j+0.5f)*RoomSize)
						-new Vector3(RoomSize * HouseWidth/2f,
						             0, RoomSize*HouseLength/2f);
				if(DebugOverlay){
					var panel = Instantiate(DebugOverlaySquare,
					                        floor[i,j].Position + 0.5f* Vector3.down,
					                        Quaternion.Euler(90,0,0)) as GameObject;
					panel.GetComponent<Renderer>().material.color = new Color(((i+j)%2)*256,((i+j)%2)*256,((i+j)%2)*256);
					panel.transform.localScale = panel.transform.localScale*RoomSize;
				}
			}
		}
	}
	#endregion
}
