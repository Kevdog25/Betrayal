using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BetterClicks : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {

	public delegate void VoidListener();

	public VoidListener PointerUpListeners;
	public VoidListener PointerDownListeners;


	public void OnPointerUp(PointerEventData data){
		if(PointerUpListeners != null)
			PointerUpListeners();
	}
	
	public void OnPointerDown(PointerEventData data){
		if(PointerDownListeners != null)
			PointerDownListeners();
	}
}
