using UnityEngine;
using System.Collections;

public class Parabola : ICurve3D {

	Vector3 startPosition;
	Vector3 apex;

	public Parabola(params Vector3[] fixedPoints){
		SetParams (fixedPoints[0],fixedPoints[1]);
	}

	public Vector3 Eval(float t){
		float x = t * apex.x;
		float z = t * apex.z;
		float y = apex.y * (1f - (t - 1f) * (t - 1f));

		return startPosition + new Vector3(x,y,z);
	}

	public void SetParams(params Vector3[] fixedPoints){
		if (fixedPoints.Length != 2) {
			Debug.LogError ("Invalid number of fixed points for parabola.");
		} else {
			startPosition = fixedPoints[0];
			apex = fixedPoints[1];
		}
	}
}
