using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {

	[Header("Menus")]
    [Tooltip("Menu to display when the game isn't running.")]
    public GameObject StartMenu;
    [Tooltip("Menu canvas to display during pause." +
        "Must be set to inactive as default.")]
    public GameObject PauseMenu;

	[Header("Network Settings")]
	public GameObject NetworkManager;
	public RectTransform HostListAnchor;
	public Button HostButtonType;

    private GameObject[] menuList;
    private GameManager gameManager;
	private NetworkManager networkManager;
	private Button[] hostButtons;

	// Use this for initialization
	void Start () {
        menuList = new GameObject[]
                {
                StartMenu,
                PauseMenu
                };

        gameManager = GetComponent<GameManager>();
		networkManager = GetComponent<NetworkManager> ();
	}
	

    /// <summary>
    /// Deactivate all menus
    /// </summary>
    public void DeactivateMenus()
    {
        foreach (var menu in menuList)
        {
            menu.SetActive(false);
        }
    }

    /// <summary>
    /// Sets up the menu scheme for the paused game.
    /// </summary>
    public void Pause()
    {
        PauseMenu.SetActive(true);
    }

    /// <summary>
    /// Reverts the menu scheme from pasued to playing.
    /// </summary>
    public void UnPause()
    {
        PauseMenu.SetActive(false);
    }

    #region Button Functionality
    /// <summary>
    /// Resets the menus to the initial starting points.
    /// </summary>
    public void ResetToStart()
    {
        gameManager.EndGame();
        DeactivateMenus();
        StartMenu.SetActive(true);
    }

    /// <summary>
    /// Closes the application through the game manager.
    /// </summary>
    public void QuitGame()
    {
        gameManager.CloseGame();
    }

	#region Network
	/// <summary>
	/// Starts a server through the NetworkManager.
	/// </summary>
	public void Host(){
		networkManager.StartServer ();
	}

	/// <summary>
	/// Refreshes the hosting servers list.
	/// </summary>
	public void RefreshList(){
		// Request a refresh from the NetworkManager.
		// Provide callback for when it is completed.
		// x is of type HostData[]
		networkManager.RefreshList ((x) => SetButtons(x));
	}

	/// <summary>
	/// Sets the buttons for the available hosts
	/// </summary>
	/// <param name="hostData">Host data.</param>
	public void SetButtons(HostData[] hostData){
		// If there are already buttons, remove them.
		if (hostButtons != null) {
			for (var i = 0; i < hostButtons.Length; i++) {
				DestroyImmediate (hostButtons [i]);
			}
		}

		// Make a new list of buttons
		hostButtons = new Button[hostData.Length];
		// Populate that list
		for (var i = 0; i < hostData.Length; i++) {
			var hostButtonClone = Instantiate(HostButtonType);
			hostButtons[i] = hostButtonClone;
			
			var buttonTrans = (RectTransform)hostButtonClone.transform;
			buttonTrans.anchorMin = new Vector2(0,0);
			buttonTrans.anchorMax = new Vector2(1,1);
			buttonTrans.position += 
				new Vector3(0,buttonTrans.rect.height*i,0);
			buttonTrans.SetParent(HostListAnchor.transform,false);

			// Grab a reference to the host data being used.
			// This is necessary to use as a const in the lambda expression.
			HostData temp = hostData[i];
			
			hostButtonClone.onClick.AddListener(() => ConnectTo(temp));

			Text buttonText = hostButtonClone.GetComponentInChildren<Text>();
			buttonText.text = temp.gameName;
		}
	}

	/// <summary>
	/// Proxy for NetworkManager.ConnectTo
	/// </summary>
	/// <param name="hostData">Host data.</param>
	public void ConnectTo(HostData hostData){
		networkManager.ConnectTo (hostData);
	}
	#endregion
    #endregion
}
