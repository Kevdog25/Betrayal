using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour {
	// Delegate for handling callbacks to the menu manager
	// regarding network host display
	public delegate void OnRefreshCallBack(HostData[] hostData);

	public string gameType;

	private bool refreshing;
	OnRefreshCallBack refreshCallBack;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		if (refreshing) {
			// Continuously poll the master server for the host list.
			HostData[] hostData = MasterServer.PollHostList();
			if(hostData.Length > 0){
				refreshing = false;
				// Now that it is refreshed, callback the menu manager.
				refreshCallBack(hostData);
			}
		}
	}


	/// <summary>
	/// Connects to server with hostdata data.
	/// </summary>
	/// <param name="data">data.</param>
	public void ConnectTo(HostData data){
		Network.Connect (data);
		Debug.Log ("Connected to: " + data.ToString ());
	}

	/// <summary>
	/// Starts the server.
	/// </summary>
	public void StartServer(){
		Debug.Log ("Starting Server");
		Network.InitializeServer (2, 25000, !Network.HavePublicAddress());
		MasterServer.RegisterHost (gameType,"TestGame_Kevin","This is a network test");
	}

	/// <summary>
	/// Handles events from the master server.
	/// </summary>
	/// <param name="e">Event.</param>
	void OnMasterServerEvent(MasterServerEvent e){
		if (e == MasterServerEvent.RegistrationSucceeded) {
			Debug.Log("Registered Server");
		}
	}

	/// <summary>
	/// Refreshs the list. Sets the callback method to be called when
	/// the list has been refreshed.
	/// </summary>
	/// <param name="callBack">Method to callback once refreshed.</param>
	public void RefreshList(OnRefreshCallBack callBack){
		// Save a reference to the method
		refreshCallBack = callBack;
		// Log network activity
		Debug.Log ("Refreshing");
		// Request up to date list
		MasterServer.RequestHostList (gameType);
		refreshing = true;
	}
}
