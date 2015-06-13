using UnityEngine;
using System.Collections;

public class FaceCamera : MonoBehaviour {

	public Transform cameraTarget;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 target = cameraTarget.position - transform.position;
		transform.LookAt (transform.position - target);
	}
}
