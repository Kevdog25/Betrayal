using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This class is designed to create 2D arrays of rooms.
/// </summary>
/// <remarks>The methods in this class assume that the rooms occupy
/// a single element of a 2D array. Also, that each room can only connect
/// to the 4 directions +Z,+X,-Z,-Z. Does not support arbitrary room shape
/// or layout.</remarks>
public class PathCreator : MonoBehaviour {

	#region Public Variables
	[Range(0,1)]
	[Tooltip("The probability that it will choose the best path when meandering.")]
	public float MeanderRate;
	[Range(0,1)]
	[Tooltip("The probability that branches will connect upon " +
		"running into another branch.")]
	public float ConnectOnBranchTruncate;
	#endregion
	
	/// <summary>
	/// Randomizes the floor.
	/// Enforces full doorification.
	/// Requires that the floor to randomize is already 
	/// populated with inactive <see cref="Room"/>s
	/// </summary>
	/// <param name="floor">Floor.</param>
	public void RandomizeFloor(Room[,] floor){
		int floorWidth = floor.GetLength (0);
		int floorHeight = floor.GetLength (1);
		for (int i=0; i<floorWidth; i++) {
			for (int j=0; j<floorHeight; j++) {
				if (Random.value > 0.5) {
					floor [i, j].Set = true;
				} else {
					floor [i, j].Set = false;
				}
			}
		}
		
		for (int i=0; i<floorWidth; i++) {
			for (int j=0; j<floorHeight; j++) {
				Room center = floor[i,j];
				if(i > 0)
					center.OpenTo(floor[i-1,j]);
				if(i < floorWidth-1)
					center.OpenTo(floor[i+1,j]);
				if(j > 0)
					center.OpenTo(floor[i,j-1]);
				if(j < floorHeight - 1)
					center.OpenTo(floor[i,j+1]);
			}
		}
	}

	/// <summary>
	/// Meander from the start room to the end room, making 
	/// connections and creating rooms while traveling.
	/// </summary>
	/// <param name="start">Start.</param>
	/// <param name="end">End.</param>
	/// <param name="floor">Floor.</param>
	public void Meander(int[] start,int[] end,Room[,] floor){
		// Get the bounds of the room
		int width = floor.GetLength (0);
		int height = floor.GetLength (1);
		//Set the start room.
		floor [start [0], start [1]].Set = true;

		int[] current = start;
		int[] next;

		// Run until the algorithm converges to the end point.
		// Currently this happends on a sketchy timescale.
		int iter = 0;
		int maxIters = 300;
		bool foundIt = false;
		while (!foundIt && iter < maxIters) {
			// Update next move
			next = NextMove(current,end);

			int i = next[0];
			int j = next[1];

			// Check if the next room is in bounds.
			if(0 <= i && i < width){
				if(0 <= j && j < height){
					Room currentRoom = floor[current[0],current[1]];
					Room nextRoom = floor[next[0],next[1]];
					if(!nextRoom.Set){
						nextRoom.Set = true;
						currentRoom.OpenTo(nextRoom);
						nextRoom.OpenTo(currentRoom);
					}
					current = next;
				}
			}
			foundIt = (current[0] == end[0]) && (current[1] == end[1]);
			iter++;
		}
	}

	/// <summary>
	/// Branches paths from the node.
	/// Stops paths when they run into something.
	/// </summary>
	public void BranchNode(int[] startNode,Room[,] floor){

		floor[startNode[0],startNode[1]].SetDoors(new int[]{1,1,1,1});
		floor[startNode[0],startNode[1]].Set = true;

		// Keep a list of the open nodes to extend
		var openNodes = new List<int[]>();
		openNodes.Add(startNode);

		while(openNodes.Count > 0){
			var nextList = new List<int[]>();

			// For each open node, expand it and handle the resulting connections.
			for(var i = 0; i < openNodes.Count;i++){
				int[] node = openNodes[i];
				Room thisRoom = floor[node[0],node[1]];

				// Randomize the number of doors in the room.
				// Choose from a geometric distribution.
				int nDoors = 1;
				for(var d = 0; d <= 3;d++){
					nDoors++;
					if(Random.value < 0.8){
						break;
					}
				}
				thisRoom.NDoors = nDoors;

				List<int[]> possible = GetPossibleMoves(node,floor);
				for(var j=0;j<possible.Count;j++){
					int[] temp = possible[j];

					// If the next step is backwards, ignore it
					if(temp[0]==node[0] && temp[1]==node[1])
						continue;
					// If it is a new room, then handle it
					Room nextRoom = floor[temp[0],temp[1]];
					// If it is not set already
					if(!nextRoom.Set){
						nextRoom.Set = true;
						nextRoom.OpenTo(thisRoom);
						nextList.Add (temp);
					}else{
						// If the room is not already connected to this one.
						if(!nextRoom.IsOpenTo(thisRoom)){
							if(Random.value < ConnectOnBranchTruncate){
								nextRoom.OpenTo(thisRoom);
							} else{
								thisRoom.CloseTo(nextRoom);
							}
						}
					}
				}
			}

			// Assign the next list of nodes to work with.
			openNodes = nextList;
		}

		// Finally, make sure that no doors are open to the edge.
		TrimEdges(floor);
	}

	/// <summary>
	/// Attempts to choose the next move to get to the target
	/// in a non-direct way.
	/// </summary>
	/// <returns>The next position.</returns>
	/// <param name="current">Current.</param>
	/// <param name="target">Target.</param>
	int[] NextMove(int[] current, int[] target){

		if (current.Equals (target)) {
			Debug.Log("Current room index is target.");
			return current;
		}

		var allowedDirections = new Vector2[]
			{Vector2.up,Vector2.right,-Vector2.up,-Vector2.right};

		// Guess the best direction and enumerate to find it.
		Vector2 bestDirection = Vector2.up;
		var targetDirection = 
			new Vector2 (target [0] - current [0], target [1] - current [1]); 
		foreach (var dir in allowedDirections) {
			if(Vector2.Dot(targetDirection,dir) > 
			   Vector2.Dot(targetDirection,bestDirection)){
				bestDirection = dir;
			}
		}

		// Roll to see if any of the other directions are chosen instead
		// of the "best" one.
		if (Random.value < 1 - MeanderRate) {
			Vector2 choice;
			do{
			 	choice = allowedDirections[Random.Range(0,allowedDirections.Length)];
			}while(choice.Equals(bestDirection));
			bestDirection = choice;
		}

		// Return the decided direction. This is no longer the 
		// "Best" one as the name may suggest.
		return new int[]{current [0] + (int)bestDirection.x,
						current [1] + (int)bestDirection.y};
	}

	/// <summary>
	/// Returns the possible moves from a position given the floor and the room's doors.
	/// </summary>
	/// <returns>The possible moves.</returns>
	/// <param name="spot">Spot.</param>
	/// <param name="floor">Floor.</param>
	List<int[]> GetPossibleMoves(int[] spot,Room[,] floor){
		var possibleMoves = new List<int[]>();
		int width = floor.GetLength(0);
		int length = floor.GetLength(1);
		int x = spot[0];
		int y = spot[1];
		int[] doors = floor[x,y].GetDoors();
		if(doors.Length != 4){
			Debug.LogWarning("Attempting to use PathCreator on improper room shape");
		}
		// If the above room is valid and available.
		if(doors[0]==1 && (y + 1 < length)){
			possibleMoves.Add(new int[]{x,y+1});
		}
		// If the above room is valid and available.
		if(doors[1]==1 &&  (x + 1 < width)){
			possibleMoves.Add(new int[]{x+1,y});
		}
		// If the above room is valid and available.
		if(doors[2]==1 && (y > 0)){
			possibleMoves.Add(new int[]{x,y-1});
		}
		// If the above room is valid and available.
		if(doors[3]==1 && (x > 0)){
			possibleMoves.Add(new int[]{x-1,y});
		}
		return possibleMoves;
	}

	/// <summary>
	/// Trims the edges of the floor so no rooms "leave" the floor.
	/// </summary>
	/// <param name="floor">Floor.</param>
	void TrimEdges(Room[,] floor){
		int width = floor.GetLength(0);
		int length = floor.GetLength(1);
		for(var i = 0; i < width;i++){
			int[] doors = floor[i,0].GetDoors();
			doors[2] = 0;
			doors = floor[i,length-1].GetDoors();
			doors[0] = 0;
		}
		for(var i = 0; i < width;i++){
			int[] doors = floor[0,i].GetDoors();
			doors[3] = 0;
			doors = floor[width-1,i].GetDoors();
			doors[1] = 0;
		}

	}
}
