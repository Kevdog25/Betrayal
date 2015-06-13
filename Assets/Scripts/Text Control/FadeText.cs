using UnityEngine;
using System.Collections;

public class FadeText : MonoBehaviour {

	[Header("Duration and Fade Time.")]
	[Tooltip("The full time to complete the arc.")]
	public float FullTime;
	[Tooltip("The fraction of completion time it is visible for")]
	public float FadeTimeFraction;
	[Header("Movement and Position.")]
	[Tooltip("The range of starting positions to randomly choose from.")]
	public Vector3 StartPositionRange;
	[Tooltip("The base target position for the text to aim at")]
	public Vector3 TargetPositionBase;
	[Tooltip("The range of targeting positions to randomly choose from.")]
	public Vector3 TargetPositionRange;

	float lifeTime;
	Vector3 startingPosition;
	Vector3 targetPosition;
	CanvasGroup canvasGroup;
	ICurve1D fadeCurve;
	ICurve3D trajectory;
	/// <summary>
	/// Initialize the starting position and target position
	/// </summary>
	void Start () {
		startingPosition = 
			new Vector3 
				(
				(1-2*Random.value),
				(1-2*Random.value),
				(1-2*Random.value)
				);
		transform.position = startingPosition;

		targetPosition = TargetPositionBase +
			new Vector3
				(
				TargetPositionRange.x * (1 - 2 * Random.value),
				TargetPositionRange.y * (1 - 2 * Random.value),
				TargetPositionRange.z * (1 - 2 * Random.value)
				);

		canvasGroup = GetComponent<CanvasGroup> ();
		Debug.Log (targetPosition);
		fadeCurve = new SharpDrop ();
		trajectory = new Parabola (startingPosition,targetPosition);
	}
	
	// Update is called once per frame
	void Update () {
		lifeTime += Time.deltaTime;
		if (lifeTime > FullTime*FadeTimeFraction) {
			DestroyImmediate(gameObject);
			return;
		}
		transform.localPosition = trajectory.Eval (2*lifeTime/FullTime);
		canvasGroup.alpha = 
			fadeCurve.Eval (lifeTime / (FullTime * FadeTimeFraction));
	}
}
