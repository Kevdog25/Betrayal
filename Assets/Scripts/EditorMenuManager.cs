using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class EditorMenuManager : MonoBehaviour {

	#region Public Variables
	[Tooltip("Editor that controlls the editor environment.")]
	public EditorManager Editor;
	[Tooltip("Button used to put into the lists of things.")]
	public Button LayoutButton;
	[Tooltip("Button that can be picked up and dragged for the" +
		"component list.")]
	public Button ComponentButton;
	[Header("Scenario picking.")]
	public RectTransform ScenarioList;

	[Header("Component picking.")]
	public RectTransform ComponentList;

	[Header("Editing overlay.")]
	public RectTransform FloorList;
	public GameObject FloorToggle;

	[Header("Scenario view.")]
	public InputField ScenarioName;
	public InputField ScenarioHouseWidth;
	public InputField ScenarioHouseLength;
	public InputField ScenarioRoomSize;
	public Image SameNameRedThing;
	public Button SaveButton;

	[Header("Component Editing.")]
	public RectTransform ExposedComponentArea;
	public RectTransform ExposedComponentList;
	public GameObject ExposedComponent;
	public GameObject ComponentProperty;
	#endregion

	List<Button> scenarioButtons;
	List<Button> componentButtons;
	List<GameObject> componentEditItems;
	List<EditorFloorToggle> floorToggles;
	Button heldButton;
	RectTransform currentComponentView;
	RectTransform screen; 

	// Use this for initialization
	void Start () {
		floorToggles = new List<EditorFloorToggle>();
		scenarioButtons = new List<Button>();
		componentButtons = new List<Button>();
		componentEditItems = new List<GameObject>();
		screen = transform as RectTransform;

		// Make a new scenario button that remains on the top.
		Button newScenario = Instantiate (LayoutButton);
		newScenario.GetComponentInChildren<Text>().text = "Add Scenario";
		var buttonTrans = newScenario.transform as RectTransform;
		buttonTrans.SetParent(ScenarioList, false);
		newScenario.onClick.AddListener(() => Editor.ShowScenario());
		Editor.LoadScenarios();
		Editor.ShowScenario();
		Editor.LoadComponents();
		//scenarioButtons.Add(newScenario);
	}

	void Update(){
		if(heldButton != null){
			var trans = heldButton.transform as RectTransform;
			trans.position = Input.mousePosition;
		}
	}

	/// <summary>
	/// Adds the floor to the sidebar list.
	/// </summary>
	public void AddFloor(){
		// Create a new floor button.
		GameObject listItem = Instantiate (FloorToggle);

		// Set it as a child of the scroll list.
		var itemTrans = listItem.transform as RectTransform;
		itemTrans.SetParent(FloorList.transform,false);
		// Set this one to be the new bottom.
		itemTrans.SetAsFirstSibling();

		// Get the script for easy child access.
		EditorFloorToggle editFloorToggle = listItem.GetComponent<EditorFloorToggle>();
		
		int floorIndex = floorToggles.Count;
		editFloorToggle.Text.text = string.Format("floor {0}",floorIndex+1);
		// Give the button the ability to add a new floor.
		editFloorToggle.PlusButton.onClick.AddListener(() => Editor.AddFloor());
		
		// Add the floor
		floorToggles.Add(editFloorToggle);

		SetFloorToggleListeners();
	}

	/// <summary>
	/// Removes the specified floor from the editor overlay sidebar.
	/// </summary>
	/// <param name="index">The floor to remove.</param>
	public void RemoveFloor(int index){
		GameObject removedButton = floorToggles[index].gameObject;
		floorToggles.RemoveAt(index);
		DestroyImmediate(removedButton);

		SetFloorToggleListeners();
	}

	/// <summary>
	/// Lets the floor toggle buttons control floor creation
	/// and removal.
	/// </summary>
	void SetFloorToggleListeners(){
		for(var i = 0; i < floorToggles.Count; i++){
			EditorFloorToggle editFloorToggle = floorToggles[i];

			// Only activate the top plus button.
			if(i == floorToggles.Count-1){
				editFloorToggle.PlusButton.gameObject.SetActive(true);
			} else{
				editFloorToggle.PlusButton.gameObject.SetActive(false);
			}

			// Remove the listeners so we can add new ones,
			// based on their new positions in the list.
			editFloorToggle.MinusButton.onClick.RemoveAllListeners();
			editFloorToggle.FloorToggle.onValueChanged.RemoveAllListeners();

			// Give the button the ability to remove the floor.
			// Get a handler for i. lambda delegates work strangely with loops.
			int floorIndex = i;
			editFloorToggle.MinusButton.onClick.AddListener(() => Editor.RemoveFloor(floorIndex));
			// Allow the toggle to turn on and off floors.
			editFloorToggle.FloorToggle.onValueChanged.AddListener((bool arg0) => Editor.ToggleFloor(floorIndex,arg0));
		}
	}

	/// <summary>
	/// Updates the list of components based on the 
	/// </summary>
	/// <param name="components">Components.</param>
	public void DisplayComponentsList(List<ScenarioComponent> components){
		for(var i = 0; i < componentButtons.Count;i++){
			DestroyImmediate(componentButtons[i].gameObject);
		}
		
		
		// Generate a new list of buttons
		componentButtons.Clear();
		for(var i = 0; i < components.Count; i++){
			string name = components[i].Name;
			
			Button button = Instantiate(ComponentButton);
			button.gameObject.name = string.Format("Scenario {0}",i);
			var buttonTrans = button.transform as RectTransform;
			buttonTrans.SetParent(ComponentList,false);

			button.GetComponentInChildren<Text>().text = name;
			button.GetComponent<BetterClicks>().PointerDownListeners += () => PickUpButton(name);
			
			componentButtons.Add(button);
		}
	}

	/// <summary>
	/// Picks up a component button and writes <see cref="name"/> on it.
	/// </summary>
	/// <param name="name">Name to display on it.</param>
	public void PickUpButton(string name){
		heldButton = Instantiate(ComponentButton);
		heldButton.GetComponentInChildren<Text>().text = name;
		heldButton.transform.SetParent(transform,false);

		heldButton.GetComponent<BetterClicks>().PointerUpListeners += () => DropButton();
	}

	public void DropButton(){
		Vector2 mouse = Input.mousePosition - ExposedComponentArea.position;
		mouse += ExposedComponentArea.rect.center;

		if(ExposedComponentArea.rect.Contains(mouse)){
			string componentName = heldButton.GetComponentInChildren<Text>().text;
			Editor.AddComponent(componentName);
			print ("Adding component: " + componentName);
		}
		DestroyImmediate(heldButton.gameObject);
	}

	/// <summary>
	/// Displays the scenarios in a scrollable list.
	/// Connects a button to each scenario for selection.
	/// </summary>
	public void DisplayScenariosList(Scenario[] scenarios){
		// Remove all current buttons. Except for the first,
		// which is the New Scenario button.
		for(var i = 0; i < scenarioButtons.Count;i++){
			DestroyImmediate(scenarioButtons[i].gameObject);
		}
		

		// Generate a new list of buttons
		scenarioButtons.Clear();
		for(var i = 0; i < scenarios.Length; i++){
			string name = scenarios[i].Name;

			Button button = Instantiate(LayoutButton);
			button.gameObject.name = string.Format("Scenario {0}",i);
			var buttonTrans = button.transform as RectTransform;
			buttonTrans.SetParent(ScenarioList,false);

			button.onClick.AddListener(() => Editor.ShowScenario(name));
			button.GetComponentInChildren<Text>().text = name;

			scenarioButtons.Add(button);
		}
	}

	/// <summary>
	/// Expands the scenario properties in the ScenarioView window.
	/// </summary>
	/// <param name="scenario">Scenario.</param>
	public void ViewScenario(Scenario scenario){

		// Update all the input fields
		ScenarioName.text = scenario.Name;
		ScenarioHouseWidth.text = scenario.HouseWidth.ToString();
		ScenarioHouseLength.text = scenario.HouseLength.ToString();
		ScenarioRoomSize.text = scenario.RoomSize.ToString();
	}

	/// <summary>
	/// Displays the components in the menu.
	/// </summary>
	/// <param name="components">Components.</param>
	public void ViewComponents(List<ScenarioComponent> components){
		for(var i = 0; i < componentEditItems.Count; i++){
			DestroyImmediate(componentEditItems[i]);
		}
		componentEditItems.Clear();

		for(var i = 0; i < components.Count; i++){
			ScenarioComponent comp = components[i];
			var exposedCompListItem = Instantiate(ExposedComponent);
			componentEditItems.Add(exposedCompListItem);
			var exposedTrans = exposedCompListItem.transform as RectTransform;
			exposedTrans.SetParent(ExposedComponentList,false);

			// Put the name first, and handle it a little differently.
			var nameField = Instantiate(ComponentProperty);
			nameField.transform.SetParent(exposedTrans,false);
			var nameInput = nameField.GetComponent<VariableInput>();
			nameInput.Name.text = "Name";
			nameInput.InputField.text = comp.Name;
			nameInput.InputField.onEndEdit.AddListener((x) => {comp.Name = x;});

			// Set up the input field for the string values.
			foreach(KeyValuePair<string,string> pair in comp.GetStringProperties()){
				var field = Instantiate(ComponentProperty);
				field.transform.SetParent(exposedTrans,false);
				var input = field.GetComponent<VariableInput>();
				input.Name.text = pair.Key;
				input.InputField.text = pair.Value;
				input.InputField.onEndEdit.AddListener
					(
						(value) => {Editor.SetComponentString(comp,input.Name.text,value);}
					);
			}

			// Set up the input field for the int values.
			foreach(KeyValuePair<string,int> pair in comp.GetIntProperties()){
				var field = Instantiate(ComponentProperty);
				field.transform.SetParent(exposedTrans,false);
				var input = field.GetComponent<VariableInput>();
				input.Name.text = pair.Key;
				input.InputField.text = pair.Value.ToString();
				input.InputField.onEndEdit.AddListener
					(
						(value) => {Editor.SetComponentInt(comp,input.Name.text,value);}
					);
			}
			
			// Set up the input field for the int values.
			foreach(KeyValuePair<string,float> pair in comp.GetFloatProperties()){
				var field = Instantiate(ComponentProperty);
				field.transform.SetParent(exposedTrans,false);
				var input = field.GetComponent<VariableInput>();
				input.Name.text = pair.Key;
				input.InputField.text = pair.Value.ToString();
				input.InputField.onEndEdit.AddListener
					(
						(value) => {Editor.SetComponentFloat(comp,input.Name.text,value);}
					);
			}
		}
	}


	/// <summary>
	/// Notifies the user that they are trying to overwrite 
	/// and overwrite protected file.
	/// </summary>
	public void OverwriteScenarioWarning(){
		print ("Scenario Overwrite Warning");
	}

	/// <summary>
	/// Notifies the user that they have attempted to create 
	/// a number of floors beyond the limit, and that their floor
	/// has not been created.
	/// </summary>
	public void TooManyFloorsWarning(){
		// TODO: Display a warning that the user is trying 
		// to exceed the floor limit.
	}

	/// <summary>
	/// Notifies the user that they tried to enter the wrong data type 
	/// into a component property field.
	/// </summary>
	/// <param name="additionalInfo">Description of the incorrect assignment.</param>
	public void WrongTypeWarning(string additionalInfo){
		// TODO: Display a warning that will only disapear
		// after the value is corrected.
		// There needs to be a list of all the invalid values somewhere.
	}

	/// <summary>
	/// Notifies the user that they attempted to overwrite
	/// certain component files, and tells them which ones have 
	/// not be saved.
	/// </summary>
	/// <param name="names">Names of the components that remain unsaved.</param>
	public void OverwriteComponentsWarning(List<string> names){
		// TODO: Display a warning that certain components have
		// not been saved to avoid overwriting and built in files.
	}
}
