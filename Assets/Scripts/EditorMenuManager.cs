using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class EditorMenuManager : MonoBehaviour {

	public EditorManager Editor;
	public RectTransform FloorList;
	public GameObject FloorToggle;
	public RectTransform ScenarioList;
	public Button GenericButton;

	List<Button> scenarioButtons;
	List<EditorFloorToggle> floorToggles;


	// Use this for initialization
	void Start () {
		floorToggles = new List<EditorFloorToggle>();
		//AddFloor();
	}


	public void AddFloor(int width = 0, int length = 0){
		// Deactivate all + buttons but the new one.
		for(var i = 0; i < floorToggles.Count; i++){
			floorToggles[i].PlusButton.gameObject.SetActive(false);
		}
		GameObject listItem = Instantiate (FloorToggle);
		var itemTrans = listItem.transform as RectTransform;
		itemTrans.SetParent(FloorList.transform,false);
		EditorFloorToggle editFloorToggle = listItem.GetComponent<EditorFloorToggle>();

		// Give the button the ability to call this method.
		editFloorToggle.PlusButton.onClick.AddListener(() => AddFloor());

		int floorIndex = floorToggles.Count;
		editFloorToggle.FloorToggle.onValueChanged.AddListener((bool arg0) => Editor.ToggleFloor(floorIndex,arg0));

		// Finally actually add the floor.
		Editor.AddFloor(width,length);
		floorToggles.Add(editFloorToggle);
	}

	/// <summary>
	/// Loads the scenarios from the scenarios file, and 
	/// make a list of buttons out of them.
	/// </summary>
	public void LoadScenarios(){
		// Remove all current buttons.
		if(scenarioButtons != null){
			for(var i = 0; i < scenarioButtons.Count;i++){
				DestroyImmediate(scenarioButtons[i]);
			}
		}

		// Generate a new list of buttons
		scenarioButtons = new List<Button>();
		Scenario[] scenarios = Editor.RefreshScenarios();
		for(var i = 0; i < scenarios.Length; i++){
			string name = scenarios[i].Name;

			Button button = Instantiate(GenericButton);
			var buttonTrans = button.transform as RectTransform;
			buttonTrans.SetParent(ScenarioList,false);

			button.onClick.AddListener(() => Editor.LoadScenario(name));
			button.GetComponentInChildren<Text>().text = name;
		}
	}


}
