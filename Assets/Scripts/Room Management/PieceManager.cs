using UnityEngine;
using System.Collections;

public class PieceManager : MonoBehaviour{

	public bool EnforceWallThickness;
	[Tooltip("The scale the rooms are built for.")]
	public float DefaultRoomSize;

	RoomController Room;

	public void Initialize(RoomController room){
		Room = room;
	}

	public void PackBase(){
		GameObject floor = Room.GetFloor();
		GameObject[] sides = Room.GetSides ();
		GameObject pillar = Room.AllowedPillar;
		if (floor == null ||
			sides.Length == 0 ||
		    pillar == null) {
			Debug.LogError("Failed Attempt to pack rooms. Insufficient material.");
			return;
		}

		float floorThickness = Room.FloorThickness;
		float size = Room.Size;

		//Parent the floor to this.transform
		floor.transform.parent = Room.transform;
		//Initialize the position
		floor.transform.localPosition = new Vector3(0,-floorThickness/2f,0);
		floor.transform.localScale = new Vector3 (size, floorThickness, size);

		#region Wall Packing
		for (int i = 0; i < sides.Length; i++){
			Transform wallTrans = sides[i].transform;

			float wallThickness;
			if(!EnforceWallThickness)
				wallThickness = wallTrans.localScale.z;
			else
				wallThickness = Room.WallThickness;

			//Parent the wall to this.transform
			wallTrans.parent = Room.transform;
			//Scale it in the x direction to fit the room
			wallTrans.localScale = new Vector3(
				size-2*wallThickness,
				wallTrans.localScale.y,
				wallThickness);

			//Position it clockwise around the room (4-Sides)
			float r = size/2f - wallThickness/2f;
			wallTrans.localPosition = new Vector3(0,0,r);
			wallTrans.RotateAround(Room.transform.position,
			                       Vector3.up,
			                       i * 90);

			//Make pillars to fill the open space.
			Transform pillarTrans = Instantiate(pillar).transform;
			pillarTrans.parent = Room.transform;
			pillarTrans.localScale = 
				new Vector3(wallThickness,
				            wallThickness,
				            pillarTrans.localScale.z);
			pillarTrans.localPosition = new Vector3(r,0,r);
			pillarTrans.rotation = Quaternion.Euler(-90,0,0);
			pillarTrans.RotateAround(Room.transform.position,Vector3.up,i*90);


		}
		#endregion
	}

	public void PackRoom(){
		if(Room == null){
			Debug.LogError("Uninitialied PieceManager. No room found.");
			return;
		}

		PackBase();
	}
}
