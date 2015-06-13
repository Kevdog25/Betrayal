using UnityEngine;
using System.Collections;

public class SharpDrop : ICurve1D {

	float R;
	float l;

	public SharpDrop(params float[] parameters){
		if (parameters.Length > 0)
			R = parameters [0];
		else
			R = 0.5f;
		if (parameters.Length > 1)
			l = parameters [1];
		else
			l = 0.1f;
	}

	public float Eval(float t){
		return 1f / (1f + Mathf.Exp ((t - R) / l));
	}

	public void SetParams(params float[] parameters){
		if (parameters.Length > 0)
			R = parameters [0];
		else
			R = 0.5f;
		if (parameters.Length > 1)
			l = parameters [1];
		else
			l = 0.1f;
	}
}
