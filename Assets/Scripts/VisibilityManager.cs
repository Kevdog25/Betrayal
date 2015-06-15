using UnityEngine;
using System.Collections;

public class VisibilityManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider other){
		if(other.tag == "Player"){
			PlayerController playerControl = other.gameObject.GetComponent<PlayerController>();
			if(playerControl == null){
				Debug.LogError("Object tagged with \"Player\" has no PlayerController");
			}else{
				playerControl.UpdateFloorMask(gameObject.layer);
			}
		}
	}
}
