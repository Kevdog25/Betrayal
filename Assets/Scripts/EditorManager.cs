using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;


public class EditorManager : MonoBehaviour {

	public GameObject Floor;
	public EditorMenuManager Menu;

	Scenario[] Scenarios;
	List<ScenarioComponent> AllComponents;
	List<ScenarioComponent> FilteredComponents;
	Scenario scenario;
	List<FloorController> Floors;
	const float defaultFloorHeight = 10;
	const string defaultName = "!New Scenario";
	const float defaultRoomSize = 25f;
	const int maxNumberOfFloors = 10;
	int[] defaultDimensions = new int[]{10,10};
	string currentFilter = "";
	string currentComponentType = "Bad Guy";

	// Use this for initialization
	void Start () {
		// Make and save a ScenarioComponent.
		// This is just to get them started.
		FilteredComponents = new List<ScenarioComponent>();
		ScenarioComponent scenarioComponent = new DummyComponent();
		scenarioComponent.Save();
		AllComponents = new List<ScenarioComponent>();
	}

	#region Editing Through Menu
	/// <summary>
	/// Loads the scenario for editing.
	/// Makes a new scenario if it cant find the proper one.
	/// </summary>
	/// <param name="name">Name of the scenario to load.</param>
	public void ShowScenario(string name = defaultName){
		Debug.Log("Loading: " + name);
		// Find the scenario
		bool foundIt = false;
		for(var i = 0; i < Scenarios.Length; i++){
			if(Scenarios[i].Name == name){
				scenario = Scenarios[i];
				foundIt = true;
				break;
			}
		}
		// If you didn't find it
		// make one with that name and isplay for editing.
		if(!foundIt){
			scenario = new Scenario();
			scenario.HouseWidth = defaultDimensions[0];
			scenario.HouseLength = defaultDimensions[1];
			scenario.RoomSize = defaultRoomSize;

			if(name == defaultName){
				// Keep incrementing the name until you find a suitable
				// New Scenario name.
				int nameIndex = 0;

				// If the index is 0, then dont consider it for the name
				// If it is not 0, then go ahead and name it with the number
				while(CheckName(nameIndex == 0 ? "New Scenario" : string.Format("New Scenario {0}",nameIndex))){
					nameIndex++;
				}
				scenario.Name = nameIndex == 0 ? "New Scenario" : string.Format("New Scenario {0}",nameIndex);
			}else{
				// I dont think this should happen.
				scenario.Name = name;
			}

		}

		Menu.ViewScenario(scenario);
		ShowComponents(currentComponentType);
	}

	/// <summary>
	/// Checks if this name is being used by a scenario.
	/// </summary>
	/// <returns><c>true</c>, if name was checked, <c>false</c> otherwise.</returns>
	/// <param name="name">Name.</param>
	bool CheckName(string name){
		for(var i = 0; i < Scenarios.Length; i++){
			if(name == Scenarios[i].Name){
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Tells the menu to display the components with
	/// the chosen key.
	/// </summary>
	/// <param name="type">Type.</param>
	public void ShowComponents(string type){
		currentComponentType = type;
		print (type);
		Menu.ViewComponents(scenario.Components[type]);
	}

	public void AddComponent(string componentName){
		bool foundIt = false;
		// Look throught the components to find the 
		// correct saved component.
		for(var i = 0; i < AllComponents.Count; i++){
			if(AllComponents[i].Name == componentName){
				foundIt = true;
				scenario.Components[currentComponentType].Add(AllComponents[i]);
				break;
			}
		}
		if(!foundIt){
			Debug.LogError("Cannot find component with name: " + componentName);
		}

		// Refresh the component display
		ShowComponents(currentComponentType);
	}

	/// <summary>
	/// Retrieves the scenarios from file.
	/// Then updates the Editor Menu to display them.
	/// </summary>
	public void LoadScenarios(){
		var info = new DirectoryInfo(Path.Combine(Application.dataPath,"../Scenarios/"));
		var fileInfo = info.GetFiles();
		Scenarios = new Scenario[fileInfo.Length];
		
		for(int i = 0; i < fileInfo.Length; i++){
			Debug.Log("Loading Scenario: " + fileInfo[i].Name);
			Scenarios[i] = Scenario.Load(fileInfo[i].Name);
		}

		// Update the menu to dislpay the new scenarios.
		Menu.DisplayScenariosList(Scenarios);
	}

	/// <summary>
	/// Loads all of the components in the save folder.
	/// </summary>
	public void LoadComponents(){
		var info = new DirectoryInfo(Path.Combine(Application.dataPath,"../ScenarioComponents/"));
		var fileInfo = info.GetFiles();

		if(AllComponents == null){
			AllComponents = new List<ScenarioComponent>();
		} else{
			AllComponents.Clear();
		}
		
		for(int i = 0; i < fileInfo.Length; i++){
			print("Loading Component: " + fileInfo[i].Name);
			AllComponents.Add (ScenarioComponent.Load(fileInfo[i].Name));
		}
		FilterComponents(currentFilter);
 	}
	
	/// <summary>
	/// Saves the working scenario.
	/// Also, update the scene list.
	/// </summary>
	public void SaveScenario(){
		var fromFile = Scenario.Load(scenario.Name);
		if(fromFile == null ||
		   !fromFile.OverwriteLock){
			scenario.Save();
			LoadScenarios();
		} else{
			Menu.OverwriteScenarioWarning();
		}
	}

	/// <summary>
	/// Saves the components on the current tab.
	/// Protects against overwriting 
	/// </summary>
	public void SaveComponents(){
		var workingComponents = scenario.Components[currentComponentType];
		var unsavedNames = new List<string>();

		LoadComponents();

		for(var i = 0; i < workingComponents.Count; i++){
			for(var j = 0; j < AllComponents.Count; j++){
				// Check if there are any components that would overwrite
				// the built in files.
				if(workingComponents[i].Name == AllComponents[j].Name &&
				   !workingComponents[i].Equals(AllComponents[j]) &&
					AllComponents[j].OverwriteLock){
					unsavedNames.Add(workingComponents[i].Name);
				}else{
					workingComponents[i].Save();
				}
			}
		}

		LoadComponents();
		if(unsavedNames.Count > 0){
			Menu.OverwriteComponentsWarning(unsavedNames);
		}
	}

	/// <summary>
	/// Filters the components and only displays those.
	/// </summary>
	public void FilterComponents(string filter = ""){
		currentFilter = filter;
		FilteredComponents.Clear();

		// Look through all the string aspects of 
		// the components.
		for(var i = 0; i < AllComponents.Count; i++){
			string[] tags = AllComponents[i].GetTags();
			print (Utility.ToString(tags));
			bool inIt = false;
			int j = 0;

			// If the name contains the filter,
			// add it to the list.
			if(AllComponents[i].Name.Contains(filter)){
				inIt = true;
			}
			while(!inIt && j < tags.Length){

				// If any of the tags contains the filter
				// Then add it to the list.
				if(tags[j].Contains(filter)){
					inIt = true;
				}

				j++;
			}

			if(inIt){
				FilteredComponents.Add(AllComponents[i]);
			}
		}

		// Finally, update the menu to display the filtered content
		Menu.DisplayComponentsList(FilteredComponents);
	}
	#endregion

	#region Editing Through World
	/// <summary>
	/// Adds the floor.
	/// Checks to make sure there aren't too many rooms.
	/// </summary>
	/// <param name="width">Width.</param>
	/// <param name="length">Length.</param>
	public void AddFloor(int width = 0, int length = 0, Room[,] fixedRooms = null){

		// Check if there are too many floors.
		if(Floors.Count >= maxNumberOfFloors){
			Menu.TooManyFloorsWarning();
			return;
		}

		// If the size of the floor is unspecified, just
		// use the dimensions of the lower floor.
		if(Floors.Count > 1){
			var top = Floors[Floors.Count-1];
			if(width == 0)
				width = top.Width;
			if(length == 0)
				length = top.Length;
		}else{
			if(width == 0)
				width = scenario.HouseWidth;
			if(length == 0)
				length = scenario.HouseLength;
		}

		// Create a real GameObject floor to look at
		// and for the player to edit.
		GameObject floor = Instantiate(Floor);
		floor.name = string.Format("Floor{0}",Floors.Count+1);
		FloorController floorControl = floor.GetComponent<FloorController>();
		floorControl.Initialize(width,length,scenario.RoomSize,true);

		// If any initial rooms are specified, set those.
		// If not, then it will be initialized to empty.
		if(fixedRooms != null){
			floorControl.SetFloor(fixedRooms);
		}
		Floors.Add (floorControl);

		Menu.AddFloor();
	}

	/// <summary>
	/// Removes the index+1'th floor. First floor is index = 0
	/// </summary>
	/// <param name="i">The index of the floor to remove.</param>
	public void RemoveFloor(int index){
		float removedHeight = Floors[index].FloorHeight;
		GameObject removedFloor = Floors[index].gameObject;
		Floors.RemoveAt(index);
		DestroyImmediate(removedFloor);
		
		// Lower each of the higher floors to fill the gap.
		for(var i = index; i < Floors.Count; i++){
			Floors[i].transform.position += Vector3.down * removedHeight;
		}

		Menu.RemoveFloor(index);
		if(Floors.Count == 0){
			AddFloor();
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

	/// <summary>
	/// Opens the current scenario for editing in the game view.
	/// </summary>
	public void EditCurrentScenario(){

		// Check if you need to make new floors, or just set
		// from the existing scenario data
		Floors = new List<FloorController>();
		if(scenario.FixedRooms == null ||
		   scenario.FixedRooms.Length == 0){
			AddFloor();
		}else{
			// Add a floor for each of the "floors" in the scneario.
			for(var i = 0; i < scenario.FixedRooms.Length; i++){
				AddFloor(scenario.FixedRooms[i].GetLength(0),
				         scenario.FixedRooms[i].GetLength(1),
				         scenario.FixedRooms[i]);
			}
		}
	}
	#endregion

	#region Scenario Values From Input Fields
	/// <summary>
	/// Sets the name of the working scenario.
	/// </summary>
	public void SetName(string value){
		scenario.Name = value;
	} 
	
	/// <summary>
	/// Sets the width of the house in the working scenario.
	/// </summary>
	/// <param name="value">Value.</param>
	public void SetWidth(string value){ int.TryParse(value,out scenario.HouseWidth); }

	/// <summary>
	/// Sets the length of the house in the working scenario.
	/// </summary>
	/// <param name="value">Value.</param>
	public void SetLength(string value){ int.TryParse(value,out scenario.HouseLength); }

	/// <summary>
	/// Sets the size of the room of the working scenario.
	/// </summary>
	/// <param name="value">Value.</param>
	public void SetRoomSize(string value){ float.TryParse(value,out scenario.RoomSize); }

	#endregion

	#region Component Values From Input Fields
	public void SetComponentString(ScenarioComponent comp,string variable,string value){
		comp.SetString(variable,value);
	}

	public void SetComponentInt(ScenarioComponent comp,string variable,string value){
		int intValue;
		// Try to parse the value into the desired type.
		if(int.TryParse(value,out intValue)){
			comp.SetInt(variable,intValue);
		}
		else{
			// If that fails, inform the user by evoking
			// a wrong type warning.
			Menu.WrongTypeWarning(string.Format("{0} expected an int, but got {1}",variable,value));
		}
	}

	public void SetComponentFloat(ScenarioComponent comp,string variable,string value){
		float floatValue;
		// Try to parse the value into the desired type.
		if(float.TryParse(value,out floatValue)){
			comp.SetFloat(variable,floatValue);
		}
		else{
			// If that fails, inform the user by evoking
			// a wrong type warning.
			Menu.WrongTypeWarning(string.Format("{0} expected a float, but got {1}",variable,value));
		}
	}

	#endregion
}
