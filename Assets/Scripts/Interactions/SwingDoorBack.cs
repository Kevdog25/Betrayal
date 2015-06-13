using UnityEngine;
using System.Collections;

public class SwingDoorBack: Interactable {
	
	Animator anim;
	DoorController doorControlls;

	void Start(){
		anim = GetComponentInParent<Animator> ();
		doorControlls = GetComponentInParent<DoorController>();
	}
	
	public override void Interact(GameObject player){
		//If the the door is open, close it
		if(anim.GetBool("OpenFront") || anim.GetBool("OpenBack")){
			anim.SetBool("OpenBack",false);
			anim.SetBool("OpenFront",false);
		} 
		//If the door is closed, open it backwards.
		else{
			anim.SetBool ("OpenBack", true);
		}

		// Notify the door that it has been opened
		doorControlls.OnOpen();
	}
}
