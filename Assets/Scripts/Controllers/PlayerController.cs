using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {

	#region Public Variables
	public float Speed;
	public float JumpSpeed;
	public float FallSpeed;
	public int Layer;
	#endregion

	//public GameObject textCanvas;

	//DisplayText textDisplay;

	#region Private Variables
	Rigidbody rigid;
	Vector3 movement;
	int floorMask;
	float camRayLength = 150f;
	List<Interactable> Interactions;
	#endregion


	// Use this for initialization
	void Start () {
		Interactions = new List<Interactable>();
		floorMask = LayerMask.GetMask ("Floor");
		rigid = GetComponent<Rigidbody> (); 
		//textDisplay = textCanvas.GetComponent<DisplayText> ();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		float h = Input.GetAxisRaw ("Horizontal");
		float v = Input.GetAxisRaw ("Vertical");

		Interact();
		Move (h, v);
		Turn ();
	}

	void Move(float h, float v)
	{
		movement.Set(h,0,v);
		movement = movement.normalized * Speed * Time.deltaTime;
		if (Input.GetKey ("space"))
			movement += new Vector3 (0, JumpSpeed, 0) * Time.deltaTime;
		else if (Input.GetKey ("left shift"))
			//movement *= 3;
			movement -= new Vector3 (0, FallSpeed, 0) * Time.deltaTime;
		rigid.MovePosition (movement + transform.position);
	}

	void Turn()
	{
		Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		
		RaycastHit floorHit;
		
		if (Physics.Raycast (camRay,out floorHit,camRayLength,floorMask))
		{
			Vector3 playerToMouse = floorHit.point-transform.position;
			playerToMouse.y = 0f;
			
			Quaternion newRotation = Quaternion.LookRotation(playerToMouse);
			rigid.MoveRotation(newRotation);
		}
	}

	/// <summary>
	/// Handles any interactions based on input
	/// </summary>
	void Interact(){
		// Loop through the interactables to activate them
		for(var i = 0; i < Interactions.Count; i++){

			// Using 'e' as the default interact key.
			if(Input.GetKeyDown("e")){
				Interactions[i].Interact(gameObject);
			}
		}
	}

	/// <summary>
	/// Sets the layer mask for the camera to only display certain floors at once.
	/// If set to 0, or a layer that isnt a floor, it will simply update the camera.
	/// </summary>
	/// <param name="layer">The layer that the player is currently in.</param>
	public void UpdateFloorMask(int layer){
		int firstFloor = LayerMask.NameToLayer("Floor0");
		// If the sent layer is a valid floor,
		// then update the mask.
		int mask = 1 << 0;
		// Get the layer mask for all default usual layers.
		for(var i = 1; i < firstFloor; i++){
			mask = mask | (1 << i);
		}
		if(layer >= firstFloor){
			// Skip all the floors
			// Now add the one the player is on.
			mask |= 1 << layer;
		}
		Layer = mask;
		// Update the camera if necessary.
		// If this is the player being watched by the camera
		if(Camera.main.GetComponent<CameraController>().Target.Equals(transform)){
			Camera.main.cullingMask = mask;
		}
	}

	/// <summary>
	/// Raises the trigger enter event.
	/// Used to handle the interactables funcitonality.
	/// </summary>
	/// <param name="other">Other.</param>
	void OnTriggerEnter(Collider other){
		if (other.tag == "Interactable") {
			Interactable function = other.gameObject.GetComponent<Interactable>();
			if(function == null){
				Debug.LogError(string.Format("Found null interactable on {0}.",other.gameObject.name));
			}else{
				// If everything checks out, add it to the list
				Interactions.Add(function);
			}
		}
	}

	/// <summary>
	/// Raises the trigger exit event.
	/// Used to remove any interactables from the list.
	/// </summary>
	/// <param name="other">Other.</param>
	void OnTriggerExit(Collider other){
		if (other.tag == "Interactable") {
			Interactable function = other.gameObject.GetComponent<Interactable>();
			if(function == null){
				Debug.LogError(string.Format("Found null interactable on {0}.",other.gameObject.name));
			}else{
				// If everything checks out, add it to the list
				Interactions.Remove(function);
			}
		}
	}
}
