using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

	[Tooltip("The allowed pool of scenarios to choose from.")]
	public GameObject[] Scenarios;


	// This doesnt belong here
	public GameObject BoundingWall;

	// This is not the correct way to spawn a player
	public GameObject Player;

	#region Private Variables
	GameObject player;
	HouseManager houseManager;
	Scenario scenario;
	#endregion

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Play(Scenario inScenario){
		
		player = Instantiate (Player);
		houseManager = GetComponent<HouseManager> ();
		houseManager.Initialize (inScenario);
		scenario = inScenario;
	}

}
