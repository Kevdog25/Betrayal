using UnityEngine;
using System.Collections;

public class Room {

	#region Public Variables
	/// <summary>
	/// Whether or not the room is supposed to be instantiated
	/// when setting up the game.
	/// </summary>
	public bool Set;
	/// <summary>
	/// The position of the room. Currently only supports positions in
	/// the y = 0 plane.
	/// </summary>
	public Vector3 Position;
	/// <summary>
	/// The type of the room. See <see cref="HouseManager.PlaceRoom"/> for details.
	/// </summary>
	public string RoomType = "Random";
	/// <summary>
	/// The physical representation of the room.
	/// </summary>
	public GameObject RoomObject;
	#endregion

	int[] doors;
	Vector3[] doorDirections;
	int nDoors;
	//GameObject roomStyle;

	#region Properties
	public int NDoors
	{
		get{return nDoors;}
		set{SetDoorsRandomly(value);}
	}
	#endregion

	#region Constructors
	/// <summary>
	/// Initializes a new instance of the <see cref="Room"/> class.
	/// Room.Set defaults to doors != null. 
	/// </summary>
	/// <param name="inDoors">Existence of doors.</param>
	public Room(int sides = 4,int[] inDoors = null){
		// Create and set lists of doors and directions
		doors = new int[sides];
		doorDirections = new Vector3[sides];
		if (inDoors == null) {
			// Default value of doors is {0,0,...0}
			// No need to set it.
			Set = false;
		} else {
			SetDoors (inDoors);
			Set = true;
		}

		// Initialize the directions normal to the doors.
		float theta = 2*Mathf.PI/sides;
		for(var i = 0; i < sides ;i++){
			doorDirections[i] = new Vector3(Mathf.Sin(i*theta),0,Mathf.Cos(i*theta));
		}
	}
	#endregion

	#region Public Methods
	/// <summary>
	/// Sets the doors from an array of ints. [Up,Right,Down,Left]
	/// </summary>
	/// <param name="doors">Existence of doors.</param>
	public void SetDoors(int[] inDoors){
		if(inDoors.Length != doors.Length){
			Debug.LogWarning("Setting doors from an incorrectly sized" +
				"input array. Doors may not behave as expected.");
		}

		for(var i = 0; i < inDoors.Length && i < doors.Length;i++){
			doors[i] = inDoors[i];
		}

		// Recount the number of doors attached to the room.
		nDoors = 0;
		for(var i = 0; i < inDoors.Length;i++){
			if(inDoors[i] == 1)
				nDoors++;
		}
	}

	/// <summary>
	/// Gets the existence of doors as an array of ints. Clockwise from +Z
	/// </summary>
	/// <returns>The doors.</returns>
	public int[] GetDoors(){
		return doors;
	}

	/// <summary>
	/// Disconnect the specified room.
	/// </summary>
	/// <param name="room">Room.</param>
	public void CloseTo(Room room){
		Vector3 p2 = room.Position;
		// Find the door
		int index = GetBestDoor(p2-Position);
		// Close it.
		if(doors[index] == 1){
			doors[index] = 0;
			nDoors--;
		}
	}

	/// <summary>
	/// Attempts to connect this room and <see cref="room"/> with a door.
	/// Requires that they are adjacent.
	/// </summary>
	/// <param name="room">Second room.</param>
	public void OpenTo(Room room){
		Vector3 p2 = room.Position;
		// Find the door
		int index = GetBestDoor(p2-Position);
		// Open it
		if(doors[index] == 0){
			doors[index] = 1;
			nDoors++;
		}
	}

	public bool IsOpenTo(Room room){
		Vector3 p2 = room.Position;
		// Find the door
		int index = GetBestDoor(p2-Position);

		return doors[index] == 1;
	}

	/// <summary>
	/// Determines whether this instance is connected the specified room.
	/// This checks to see if the other door is open to this one as well.
	/// </summary>
	/// <returns><c>true</c> if this instance is connected the specified room; otherwise, <c>false</c>.</returns>
	/// <param name="room">Room.</param>
	public bool IsConnected(Room room){
		// Return whether or not both are open to each other
		return IsOpenTo(room) && room.IsOpenTo(this);
	}
	#endregion

	#region Private Methods
	void SetDoorsRandomly(int numDoors){
		// Do nothing if the number of doors is already
		// larger than the input
		if(numDoors <= nDoors){
			return;
		}
		// If numDoors is more than possible, open them all.
		if(numDoors >= doors.Length){
			for(var i = 0; i < doors.Length; i++){
				doors[i] = 1;
			}
		} else{
			// Find the indeces that have closed walls
			var wallIndeces = new int[4-nDoors];
			int iW = 0;
			for(var i = 0; i < doors.Length; i++){
				if(doors[i] == 0){
					wallIndeces[iW] = i;
					iW++;
				}
			}

			// Pick one of them, and then do it again.
			int index = wallIndeces[Random.Range(0,wallIndeces.Length)];
			if(doors[index] == 1){
				Debug.Log(Utility.ToString(wallIndeces));
				Debug.Log(Utility.ToString(doors));
			} else{
				doors[index] = 1;
				nDoors++;
				SetDoors(doors);
			}
			SetDoorsRandomly(numDoors);
		}
	}

	/// <summary>
	/// Gets the index of the door with closest direction.
	/// </summary>
	/// <returns>The best door.</returns>
	/// <param name="dir">Direction to door.</param>
	int GetBestDoor(Vector3 dir){
		int index = 0;
		
		// Find whichever door direction has the largest inner product
		for(var i = 1; i < doorDirections.Length; i++){
			if(Vector3.Dot (doorDirections[i],dir) > 
			   Vector3.Dot(doorDirections[index],dir)){
				index = i;
			}
		}
		return index;
	}
	#endregion
}
