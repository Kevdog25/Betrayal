using UnityEngine;
using System.Collections;


public interface ICurve1D {
	float Eval(float t);

	void SetParams(params float[] parameters);
}
