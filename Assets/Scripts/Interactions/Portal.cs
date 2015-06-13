using UnityEngine;
using System.Collections;

public class Portal : Interactable {

	public Vector3 Origin{
		get{return origin + transform.position;}
		set{origin = value;}
	}

	Vector3 portalTo;
	Vector3 origin;

	// Use this for initialization
	void Start () {
		Origin = gameObject.transform.position;
	}

	public void SetTarget(GameObject target){
		Portal targetPortal = target.GetComponent<Portal>();
		if(targetPortal == null){
			Debug.LogError("Error: Failed initialization of portal target." +
			               "GameObject.Portal could not be found");
		}else{
			portalTo = targetPortal.Origin;
		}
	}

	public void SetTarget(Portal portal){
		if(portal == null){
			Debug.LogError("Error: Failed to set portal target to null portal.");
		} else{
			portalTo = portal.Origin;  
		}	
	}

	/// <summary>
	/// Teleport the player to the linked target.
	/// </summary>
	/// <param name="player">Player to teleport.</param>
	public override void Interact(GameObject player){
		if(portalTo != origin){
			player.transform.position = portalTo;
		} else{
			Debug.LogWarning("Warning: Attempt to use portal before it was set.");
		}	
	}
}
