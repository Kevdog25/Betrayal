using UnityEngine;
using System.Collections;

public class TextureManager : MonoBehaviour {

	public GameObject LeftWall;
	public GameObject RightWall;
	public GameObject Arch;



	// Use this for initialization
	void Start () {
		Mesh mesh = Arch.GetComponent<MeshFilter> ().mesh;
		Vector2[] uvs1 = mesh.uv;
		float ratio = 1 - Arch.transform.localScale.y / RightWall.transform.localScale.z;

		for (int i=0; i<24; i+=4) {
			uvs1[i] = new Vector2(0,ratio);
			uvs1[i+1] = new Vector2(1,0);
			uvs1[i+2] = new Vector2(1,1);
			uvs1[i+3] = new Vector2(0,ratio);
		}

		//mesh.uv = uvs1;
	}
}
