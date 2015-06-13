using UnityEngine;
using System.Collections;

public class EasyScaling : MonoBehaviour {
	public Transform ScaleThisOne;

	public void SetScale(float x,float y, float z){
		ScaleThisOne.localScale = new Vector3 (x, y, z);
	}

	public void Scale(float Sx,float Sy,float Sz){
		Vector3 currentS = ScaleThisOne.localScale;
		ScaleThisOne.localScale = 
			new Vector3 (currentS.x*Sx,
			             currentS.y*Sy,
			             currentS.z*Sz);
	}
}
