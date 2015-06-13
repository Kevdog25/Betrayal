using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	//Target object for the camera to follow
	[Tooltip("Target Transform to follow/center on.")]
	public Transform Target;
	[Tooltip("The direction from the object to the camera.")]
	public Vector3 DisplacementVec;
	[Tooltip("Default distance from the object.")]
	public float Displacement;
	[Tooltip("Rate that defines the gaps between zoom positions.")]
	public float ScrollRate;
	[Tooltip("Speed associated with panning when camera lock is off.")]
	public float PanSpeed;
	public float ZoomLerpRate;
	public float PanLerpRate;
	[Tooltip("The time to recenter on the camera when the " +
		"camera lock is re-enabled.")]
	public float TimeToAlign;
	
	bool retarget;
	bool cameraLock;
	float initialHeight;
 	Vector3 oldMouse;
	Vector3 currentMouse;
	float returnTime;
	Vector3 realTarget;
	float targetDistance;

	float shouldBeAlignedByNow;

	// Use this for initialization
	void Start () {
		DisplacementVec.Normalize ();
		retarget = true;
		cameraLock = true;
		initialHeight = (DisplacementVec*Displacement).y;
		realTarget = Target.position;
		targetDistance = Displacement;
		shouldBeAlignedByNow = 0;
	}
	
	// Update is called once per frame
	void LateUpdate () {

		Pan ();
		Zoom ();
		
		transform.position = realTarget + Displacement * DisplacementVec;

		if (retarget) {
			transform.LookAt(Target.position);
			retarget = false;
		}
	}

	/// <summary>
	/// Increases or decreases the distance to the target
	/// </summary>
	void Zoom(){
		if (Input.GetAxis ("Mouse ScrollWheel") > 0) {
			targetDistance = Mathf.Clamp(ScrollRate * targetDistance,5f,100f);
		} else if (Input.GetAxis ("Mouse ScrollWheel") < 0) {
			targetDistance = Mathf.Clamp(1f/(ScrollRate) * targetDistance,5f,100f);
		}
		Displacement = Mathf.Lerp (Displacement, targetDistance, ZoomLerpRate);

	}

	/// <summary>
	/// Move the camera along the plane of constant height.
	/// Should be called every frame.
	/// </summary>
	void Pan(){
		//Get the current mouse position. Used for panning
		currentMouse = Input.mousePosition;
		float avgScreenSize = (Screen.width + Screen.height)/2f;
		currentMouse.x = currentMouse.x / avgScreenSize;
		currentMouse.y = currentMouse.y / avgScreenSize;
		float height = transform.position.y;
		
		if (Input.GetKeyDown ("f")) {
			cameraLock = !cameraLock;
		}
		//If the user is right clicking
		if (!cameraLock) {
			if(Input.GetMouseButton(1)){
				//Pan the camera based on mouse deltas
				float panX = currentMouse.x - oldMouse.x;
				float panZ = currentMouse.y - oldMouse.y;
				realTarget += -new Vector3 (panX, 0, panZ) * PanSpeed*height/initialHeight;
			}
		} else {
			//If its time to return to the character, do so
			Align();
		} 

		oldMouse = currentMouse;
	}

	void Align(){
		if (Input.GetKeyDown ("f")) {
			shouldBeAlignedByNow = Time.time + TimeToAlign;
		}

		if (Time.time > shouldBeAlignedByNow) {
			realTarget = Target.position;
		} else {
			float distanceToAlign = (realTarget - Target.position).magnitude;
			realTarget = Vector3.Lerp(realTarget,
			                          Target.position,
			                          2f*TimeToAlign/distanceToAlign);
		}
	}
}
