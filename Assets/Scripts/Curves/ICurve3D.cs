using UnityEngine;
using System.Collections;

public interface ICurve3D{
	Vector3 Eval(float t);

	void SetParams(Vector3[] fixedPoints);
}