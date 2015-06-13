using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DoorController : MonoBehaviour {

	bool HasOpened;
	OpenCallBack OpenCalls;
	public delegate void OpenCallBack();

	// Use this for initialization
	void Start () {
		HasOpened = false;
	}

	public void OnOpen(){
		// Only call the opening on first open
		if(!HasOpened && OpenCalls != null){
			OpenCalls();
			HasOpened = true;
		}
	}

	/// <summary>
	/// Adds the method to the OnOpen call.
	/// </summary>
	/// <param name="listener">Method to be called.</param>
	public void AddOpenListener(OpenCallBack listener){
		OpenCalls += listener;
	}
	
	/// <summary>
	/// Removes the method from the OnOpen call.
	/// </summary>
	/// <param name="listener">Method to be called.</param>
	public void RemoveOpenListener(OpenCallBack listener){
		OpenCalls -= listener;
	}

	public void ListenToReveal(RoomController roomControl){
		roomControl.AddRevealListener(OnReveal);
	}

	void OnReveal(){
		gameObject.SetActive(true);
	}
}
