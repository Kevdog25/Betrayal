using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

	public GameObject GameControl;

	// Bool defaults to false;
	bool isPaused;
	GameObject gameControl;
    MenuManager menuManager;
	Scenario[] Scenarios;

	// Use this for initialization
	void Awake () {
        menuManager = GetComponent<MenuManager>();
		Scenario scene = new Scenario();
		scene.Name = "EmpyScenario";
		scene.Save(Path.Combine(Application.dataPath,"../Scenarios/Empty.txt"));
		LoadScenarios();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			TogglePause ();
		}
	}

	/// <summary>
	/// Activates the game.
	/// </summary>
	public void ActivateGame(){
		// Create the game controller to run the game logic on.
		gameControl = Instantiate (GameControl);
		gameControl.SetActive (true);
		gameControl.GetComponent<GameController>().Play(Scenarios[0]);

		// Deactivate all active menus.
        menuManager.DeactivateMenus();
	}

	/// <summary>
	/// Loads all the scenarios in the scenarios file.
	/// </summary>
	void LoadScenarios(){
		var info = new DirectoryInfo(Path.Combine(Application.dataPath,"../Scenarios/"));
		var fileInfo = info.GetFiles();
		Scenarios = new Scenario[fileInfo.Length];
		for(int i = 0; i < fileInfo.Length; i++){
			Debug.Log("Loading Scenario: " + fileInfo[i].Name);
			Scenarios[i] = Scenario.Load(fileInfo[i].FullName);

		}
	}

	/// <summary>
	/// Ends the game logic and returns to start menu.
	/// </summary>
	public void EndGame(){
		// Destroy the game controller to reset any logic.
		DestroyImmediate (gameControl);

		// Upause the game
		if (isPaused) {
			TogglePause();
		}
	}

	/// <summary>
	/// Closes the game.
	/// </summary>
	public void CloseGame(){
		Application.Quit ();
	}

	/// <summary>
	/// Stops the gametime and displays pause screen.
	/// </summary>
    void TogglePause()
    {
        isPaused = !isPaused;
		if (isPaused) {
			Time.timeScale = 0;
            menuManager.Pause();
		} else {
			Time.timeScale = 1;
            menuManager.UnPause();
		}

	}
}
