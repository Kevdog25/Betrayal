using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FloorController : MonoBehaviour {

	
	[Tooltip("The height of each floor.")]
	public float FloorHeight;
	[Tooltip("The particle effects to shroud undiscovered rooms.")]
	public GameObject ShroudParticles;
	[Tooltip("How large the shroud covers are compared to the rooms.")]
	public float ShroudSizeRatio;
	public List<GameObject> AllowedRoomStyles;
	[Tooltip("A room style used as a maker when debugging the layout.")]
	public GameObject DebugRoomStyle;
	[Tooltip("Should the rooms be covered in smoke?")]
	public bool ShroudOn;
	[Tooltip("Should all rooms be visible on start?")]
	public bool VisibleOnStart;
	public int Width;
	public int Length;
    

	#region Private Variables
	Room[,] rooms;
	float roomSize;
	List<GameObject> uniqueRooms;

	PathCreator pathMaker;
	GameObject shroud;
	GameObject doors;
    GameObject roomSpaces;
	[SerializeField] GameObject DebugQuad;
	#endregion

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	/// <summary>
	/// Activates the room in the position.
	/// Sets its door array
	/// </summary>
	/// <param name="pos">Position.</param>
	/// <param name="doorArray">Door array.</param>
	public void SetRoom(int[] pos,int[] doorArray,string type = "Random"){
		rooms[pos[0],pos[1]].Set = true;
		rooms[pos[0],pos[1]].SetDoors(doorArray);
		rooms[pos[0],pos[1]].RoomType = type;
	}

	/// <summary>
	/// Overrides the current room in that index
	/// Keeps the same position, though.
	/// </summary>
	/// <remarks>
	/// To avoid technical issues, the room matrix needs to have the same relative
	/// possitions throughout the whole generation process.
	/// </remarks>
	/// <param name="pos">Position.</param>
	/// <param name="room">Room.</param>
	public void SetRoom(int[] pos, Room room){
		room.Position = rooms[pos[0],pos[1]].Position;
		rooms[pos[0],pos[1]] = room;
	}

	/// <summary>
	/// Sets all of the rooms for a whole room array.
	/// Give error if dimensions are wrong.
	/// </summary>
	/// <param name="floor">Floor.</param>
	public void SetFloor(Room[,] floor){
		if(floor.GetLength(0) != Width ||
		   floor.GetLength(1) != Length){
			Debug.LogError("Attempted to set rooms of floor from improper array.");
			return;
		}
		for(var i = 0; i < Width;i++){
			for(var j = 0; j < Length;j++){
				SetRoom(new int[]{i,j},floor[i,j]);
			}
		}
	}

	/// <summary>
	/// Initialize the specified inWidth, inLength, inRoomSize, allowedRoomStyles and inUniqueRooms.
	/// </summary>
	/// <param name="inWidth">In width.</param>
	/// <param name="inLength">In length.</param>
	/// <param name="inRoomSize">In room size.</param>
	/// <param name="allowedRoomStyles">Allowed room styles.</param>
	/// <param name="inUniqueRooms">In unique rooms.</param>
	public void Initialize(int inWidth, int inLength, float inRoomSize, bool editMode = false){

		pathMaker = new PathCreator();
		AllowedRoomStyles = new List<GameObject>();
        roomSpaces = new GameObject();
        roomSpaces.name = "RoomSpaces";
        roomSpaces.transform.SetParent(transform,false);
		shroud = new GameObject();
		shroud.name = "Shroud";
		shroud.transform.parent = transform;
		doors = new GameObject();
		doors.name = "Doors";
		doors.transform.parent = transform;
		Width = inWidth;
		Length = inLength;
		roomSize = inRoomSize;

		uniqueRooms = new List<GameObject>();

		rooms = new Room[Width,Length];
		// Initialize the room array. 
		for(int i = 0; i < Width;i++){
			for(int j = 0; j < Length;j++){
				rooms[i,j] = new Room();
				rooms[i,j].Position = 
					new Vector3((i+0.5f)*roomSize,0,(j+0.5f)*roomSize)
						-new Vector3(roomSize * Width/2f,
						             0, roomSize*Length/2f);
				if(editMode){
					GameObject quad = Instantiate(DebugQuad);
					quad.transform.position = rooms[i,j].Position;
					quad.transform.localScale = new Vector3(1,1,1/roomSize) * roomSize;
					quad.GetComponent<Renderer>().material.color = 
						new Color(((i+j)%2)*255,((i+j)%2)*255,((i+j)%2)*255);
                    quad.transform.SetParent(roomSpaces.transform,false);
				}
			}
		}
		
		// Create a box collider to detect when players enter and leave floors. 
		BoxCollider floorTrigger = GetComponent<BoxCollider>();
		
		// Set the trigger to be the size of the floor, and right in the middle;
		floorTrigger.center = new Vector3(0,FloorHeight/2f,0);
		floorTrigger.size = new Vector3(Width * roomSize,
		                                FloorHeight*0.8f,
		                                Length * roomSize);
	}


	/// <summary>
	/// Sets the layer of this and all the children.
	/// </summary>
	/// <param name="layer">Layer.</param>
	public void SetLayer(int layer){
		foreach(Transform trans in gameObject.GetComponentsInChildren<Transform>(true)){
			trans.gameObject.layer = layer;
		}
	}

	/// <summary>
	/// Generates the floor plan.
	/// </summary>
	/// <param name="floorGenSetting">The generation method.</para>
	public void GenerateFloor(int floorGenSetting = 0){

		if(rooms == null){
			Debug.LogError("Attempt to generate uninitialized floor." +
			               "Please call Floor.Initialize first.");
			return;
		}

		// Select the generation setting
		switch(floorGenSetting){
		case 0:
			pathMaker.RandomizeFloor(rooms);
			break;
		case 1:
			ConnectUniqueRooms();
			break;
		case 2:
			ReversePathFinding();
			break;
		case 3:
			BranchFromNodes();
			break;
		}

		SetRooms();
	}


	/// <summary>
	/// Initializes the rooms as actual game objects.
	/// Also places the doors and shrouds.
	/// </summary>
	void SetRooms(){
		
		// Now that the floors have been generated
		// Set the rooms
		for (int i=0; i<Width; i++) {
			for (int j=0; j<Length; j++) {
				// Cover the room with a shroud of smoke initially
				GameObject shroudClone = Instantiate(ShroudParticles);
				shroudClone.transform.localPosition += rooms[i,j].Position;
				shroudClone.transform.localScale = new Vector3(roomSize*ShroudSizeRatio,
				                                               roomSize*ShroudSizeRatio,
				                                               1);
				shroudClone.transform.SetParent(shroud.transform,false);
				// If the ShroudOn setting is enabled
				if(ShroudOn)
					shroudClone.GetComponent<ParticleSystem>().Play();
				
				if(rooms[i,j].Set){
					
					rooms[i,j].Position = 
						new Vector3((i+0.5f)*roomSize,0,(j+0.5f)*roomSize)
							-new Vector3(roomSize * Width/2f, 0, roomSize*Length/2f);
					
					// Place the room in real space.
					GameObject room = PlaceRoom(rooms[i,j]);
					// Set its parent to the parent, for categorization.
					room.transform.SetParent(transform,false);
					
					rooms[i,j].RoomObject.GetComponent<RoomController>().Shroud = shroudClone;
				}
			}
		}
		// Set up the doors
		// This must be done last
		PlaceDoors();
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
			roomStyle = Utility.RandomElement (AllowedRoomStyles);
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
			roomStyle = Utility.RandomElement (AllowedRoomStyles);
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
		roomControl.Size = roomSize;
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
	void ConnectUniqueRooms(){
		// Generate the positions of each unique room.
		List<int[]> uniquePositions = SetUniqueRooms(uniqueRooms.Count);
		
		// Loop Through and connect them.
		for(var i = 0; i < uniquePositions.Count - 1; i++){
			pathMaker.Meander(uniquePositions[i],uniquePositions[i+1],rooms);
		}
	}
	
	/// <summary>
	/// Generates a floor plan by inhibiting path creation into the corner.
	/// Ignores <see cref="pathMaker.MeanderRate"/>.
	/// </summary>
	/// <param name="floor">Floor to generate.</param>
	void ReversePathFinding(){
		// Meander away from the corner
		// Fix the meander rate to inhibit motion 
		float meanderRate = pathMaker.MeanderRate;
		pathMaker.MeanderRate = 0.1f;
		pathMaker.Meander(new int[]{0,0},new int[]{-1,-1},rooms);
		pathMaker.MeanderRate = meanderRate;
	}
	
	/// <summary>
	/// Branches from the given nodes
	/// </summary>
	/// <param name="floor">Floor.</param>
	void BranchFromNodes(){
		// Choose the positions of the unique rooms
		List<int[]> uniquePositions = SetUniqueRooms(uniqueRooms.Count);

		
		pathMaker.Branch(rooms);
		
		// Check to make sure that each important room is set.
		for(var i = 0; i < uniquePositions.Count;i++){
			int[] pos = uniquePositions[i];
			Vector3 vecPos = rooms[pos[0],pos[1]].Position;
			
			// If it isnt, then meander to it.
			if(!rooms[pos[0],pos[1]].Set){
				float minDistance = 10000;
				var closestRoom =  new int[]{0,0};

				// Find the closest room, so as to not
				// disturb the generation much.
				for(var j = 0; j < Width; j++){
					for(var k = 0; k < Length;k++){
						Vector3 d = rooms[j,k].Position - vecPos;
						if(d.magnitude < minDistance){
							minDistance = d.magnitude;
							closestRoom = new int[]{j,k};
						}
					}
				}
				pathMaker.Meander(closestRoom,pos,rooms);
			}
		}
	}
	#endregion

	/// <summary>
	/// Places the doors for the floor.
	/// This is done based on the <see cref="Room.GetDoors()"/> method.
	/// </summary>
	/// <param name="floorIndex">Index of the floor to place the doors in.</param>
	void PlaceDoors(){
		for(var i = 0; i < rooms.GetLength(0); i++){
			for(var j = 0; j < rooms.GetLength(1); j++){
				Room room = rooms[i,j];

				// If its empty, skip it.
				if(!room.Set)
					continue;

				int[] doorArray = room.GetDoors();
				RoomController roomControl = room.RoomObject.GetComponent<RoomController>();
				
				for(var k = 0; k < doorArray.Length/2;k++){
					if(doorArray[k] == 1){
						GameObject doorClone = Instantiate(roomControl.Door);
						Transform doorTrans = doorClone.transform;
						EasyScaling scaling = doorClone.GetComponent<EasyScaling>();
						//Scale it in the x direction to fit the doorway.
						scaling.Scale(roomSize/roomControl.DefaultRoomSize,1,1);
						doorTrans.position = new Vector3(1.25f*roomSize/roomControl.DefaultRoomSize,
						                                 0,
						                                 roomSize/2f) +
							room.RoomObject.transform.position;
						
						// Rotate it around the center.
						doorTrans.RotateAround(room.RoomObject.transform.position,
						                       Vector3.up,
						                       (k*360f/doorArray.Length));
						// Parent all the doors to the doors gameObject for clarity.
						doorTrans.parent = doors.transform;
						
						DoorController doorControl = doorClone.GetComponent<DoorController>();
						if(k == 0){
							var otherRoomControl = rooms[i,j+1].RoomObject.GetComponent<RoomController>();
							
							// Set up event listeners to activate on room reveal, and door open.
							roomControl.ListenForOpen(doorControl);
							otherRoomControl.ListenForOpen(doorControl);
							doorControl.ListenToReveal(roomControl);
							doorControl.ListenToReveal(otherRoomControl);
						}
						if(k == 1){
							var otherRoomControl = rooms[i+1,j].RoomObject.GetComponent<RoomController>();
							
							// Set up event listeners to activate on room reveal, and door open.
							roomControl.ListenForOpen(doorControl);
							otherRoomControl.ListenForOpen(doorControl);
							doorControl.ListenToReveal(roomControl);
							doorControl.ListenToReveal(otherRoomControl);
						}
					}
				}
				
				if(room.RoomType == "Debug"){
					// If the room type is debug, then go ahead and reveal it.
					roomControl.Reveal();
				}
			}
		}
		
	}

	/// <summary>
	/// Sets the unique rooms. This attempts to optimally set the distance
	/// between rooms.
	/// </summary>
	/// <returns>The location of the unique rooms.</returns>
	/// <param name="dimensions">Dimensions of the floor: {width,height}.</param>
	/// <param name="nRooms">Number of rooms.</param>
	List<int[]> SetUniqueRooms(int nRooms, int[] centroid = null){
		
		// If no centroid for room placement is specified, use the center of the floor.
		if(centroid == null){
			centroid = new int[]{rooms.GetLength(0)/2,rooms.GetLength(1)/2};
		}
		var list = new List<int[]> ();
		
		// Start the average position at the centroid given.
		var avgUniquePosition = new int[]{centroid[0],centroid[1]};
		
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
			      (xy[0] >= rooms.GetLength(0)) || (xy[1] >= rooms.GetLength(1)));
			
			// If it picked an unused position, then add it to the list.
			if(!list.Contains(xy)){
				list.Add(xy);
				rooms[xy[0],xy[1]].RoomType = "Unique";
				
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
}
