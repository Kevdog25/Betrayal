using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class DisplayText : MonoBehaviour {

	public Text fadingText;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void DisplayDamage(float dmg){
		Text fadingTextClone = Instantiate (fadingText);
		fadingTextClone.transform.SetParent(transform,false);
	    fadingTextClone.text = dmg.ToString ();
	}
}
