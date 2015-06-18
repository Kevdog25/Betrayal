using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;


public class EditorManager : MonoBehaviour {

	public GameObject Floor;

	Scenario[] Scenarios;
	Scenario scenario;
	List<FloorController> Floors;
	float defaultFloorHeight = 10;
	int defaultFloorDimension = 10;

	// Use this for initialization
	void Start () {
		RefreshScenarios();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void LoadNewScenario(int width, int length){
		Floors = new List<FloorController>();
		AddFloor(width,length);
	}

	public void LoadScenario(string name){

		// Find the scenario
		bool foundIt = false;
		for(var i = 0; i < Scenarios.Length; i++){
			if(Scenarios[i].Name == name){
				scenario = Scenarios[i];
				foundIt = true;
				break;
			}
		}

		// Make sure you find it.
		if(!foundIt){
			Debug.LogError("Did not find the specified scenario: " + name);
			return;
		}

		Floors = new List<FloorController>();
		AddFloor(10,10);
	}

	public Scenario[] RefreshScenarios(){
		var info = new DirectoryInfo(Path.Combine(Application.dataPath,"../Scenarios/"));
		var fileInfo = info.GetFiles();
		Scenarios = new Scenario[fileInfo.Length];
		
		for(int i = 0; i < fileInfo.Length; i++){
			Debug.Log("Loading Scenario: " + fileInfo[i].FullName);
			Scenarios[i] = Scenario.Load(fileInfo[i].FullName);
		}

		return Scenarios;
	}

	public void AddFloor(int width = 0, int length = 0){

		if(Floors.Count > 1){
			var top = Floors[Floors.Count-1];
			if(width == 0)
				width = top.Width;
			if(length == 0)
				length = top.Length;
		}else{
			if(width == 0)
				width = defaultFloorDimension;
			if(length == 0)
				length = defaultFloorDimension;
		}


		GameObject floor = Instantiate(Floor);
		FloorController floorControl = floor.GetComponent<FloorController>();
		floorControl.Initialize(width,length,inRoomSize: 25);
		Floors.Add (floorControl);
	}

	/// <summary>
	/// Removes the i'th floor. First floor is i = 1
	/// </summary>
	/// <param name="i">The index of the floor to remove.</param>
	public void RemoveFloor(int index){

		float removedHeight = Floors[index-1].FloorHeight;
		Floors.RemoveAt(index-1);
		
		// Lower each of the higher floors to fill the gap.
		for(var i = index-1; i < Floors.Count; i++){
			Floors[i].transform.position += Vector3.down * removedHeight;
		}
	}

	/// <summary>
	/// Toggles the visibility of the floor.
	/// </summary>
	/// <param name="index">Index of the floor.</param>
	public void ToggleFloor(int index,bool on){
		// Flip the activity of the floor.
		Floors[index].gameObject.SetActive(on);
	}
}
