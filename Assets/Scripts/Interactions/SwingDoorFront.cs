using UnityEngine;
using System.Collections;

public class SwingDoorFront : Interactable {
	
	Animator anim;
	DoorController doorControlls;
	
	void Start(){
		anim = GetComponentInParent<Animator> ();
		doorControlls = GetComponentInParent<DoorController>();
	}
	
	public override void Interact(GameObject player){
		//If the the door is open, close it, if you are on the outside of it
		if(anim.GetBool("OpenBack") || anim.GetBool("OpenFront")){
			anim.SetBool("OpenBack",false);
			anim.SetBool("OpenFront",false);
		} 
		//If the door is closed, open it forwards.
		else{
			anim.SetBool ("OpenFront", true);
		}

		// Notify the door that it has been opened
		doorControlls.OnOpen();
	}
}
