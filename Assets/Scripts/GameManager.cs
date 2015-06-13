using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	public GameObject GameControl;

	private bool isPaused = false;
	private GameObject gameControl;
    private MenuManager menuManager;

	// Use this for initialization
	void Awake () {
        menuManager = GetComponent<MenuManager>();
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
		//Create the game controller to run the game logic on.
		gameControl = Instantiate (GameControl);
		gameControl.SetActive (true);

		//Deactivate all active menus.
        menuManager.DeactivateMenus();
	}

	/// <summary>
	/// Ends the game logic and returns to start menu.
	/// </summary>
	public void EndGame(){
		//Destroy the game controller to reset any logic.
		DestroyImmediate (gameControl);

		//Upause the game
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
