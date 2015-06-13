using UnityEngine;
using System.Collections;

public class RoomController : MonoBehaviour {

	public delegate void RevealCallBack();

	#region Public Variables
	public float Size;
	[Tooltip("The size the room is build to")]
	public float DefaultRoomSize;
	public float FloorThickness;
	public float WallThickness;
	public GameObject[] AllowedWalls;
	public GameObject[] AllowedDoors;
	public GameObject[] AllowedFloors;
	public GameObject[] AllowedFeatures;
	public GameObject AllowedPillar;
	public GameObject Door;
	public GameObject Shroud;

	#endregion

	#region Private Variables
	int Sides = 4;
	PieceManager pieceManager;
	GameObject[] selectedSides;
	GameObject selectedFloor;
	GameObject[] selectedFeatures;
	int[] isDoor;
	RevealCallBack RevealCallback;
	#endregion

	void Awake () {
		GetComponent<BoxCollider> ().size = Vector3.one * Size;
		pieceManager = GetComponent<PieceManager> ();
		pieceManager.Initialize (this);
		isDoor = new int[Sides];
	}

	/// <summary>
	/// Sets up walls and parents to this. Wall prefab must start facing Y+.
	/// </summary>
	/// <param name="isDoor">Labeling whether or not there is a door. </param> 
	public void SetUp(int[] inIsDoor){
		//Select the sides
		isDoor = inIsDoor;
		selectedSides = new GameObject[Sides];
		for (int i = 0; i < Sides; i++) {
			if(isDoor[i] == 1){
				selectedSides[i] = 
					Instantiate(Utility.RandomElement(AllowedDoors));
			}else{
				selectedSides[i] = 
					Instantiate(Utility.RandomElement (AllowedWalls));
			}
		}
		//Select the floor
		var floor = (GameObject)Utility.RandomElement (AllowedFloors);
		selectedFloor = Instantiate (floor);

		//Pack the walls and floor into square
		pieceManager.PackRoom ();
	}

	/// <summary>
	/// Returns the sides as an array
	/// </summary>
	/// <returns>The sides.</returns>
	public GameObject[] GetSides(){
		return selectedSides;
	}

	public GameObject GetFloor(){
		return selectedFloor;
	}

	public int[] GetIsDoor(){
		return isDoor;
	}

	public void Reveal(){
		var particles = Shroud.GetComponent<ParticleSystem>();
		particles.playbackSpeed = 24;
		particles.Stop();
		gameObject.SetActive(true);
		if(RevealCallback != null){
			RevealCallback();
		}
	}

	public void ListenForOpen(DoorController doorControlls){
		doorControlls.AddOpenListener(Reveal);
	}

	public void AddRevealListener(RevealCallBack listener){
		RevealCallback += listener;
	}
}
