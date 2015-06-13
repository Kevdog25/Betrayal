using UnityEngine;
using System.Collections;

public class ResizeTexture : MonoBehaviour {

	public Vector2 NaturalTextureSize;

	// Use this for initialization
	void Start () {
		Renderer renderer = GetComponent<Renderer> ();
		Vector3 dimensions = transform.localScale;
		Debug.Log (dimensions);
		renderer.material.mainTextureScale = new Vector2(
			dimensions.x/NaturalTextureSize.x,
			dimensions.y/NaturalTextureSize.y);

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
