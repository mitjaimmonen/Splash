using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour {

	void OnTriggerEnter(Collider other)
	{
		//If collided object has any class with this function, it will handle the event itself.
		other.gameObject.BroadcastMessage("OnWaterTrigger", SendMessageOptions.DontRequireReceiver);
	}
}
