using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPlatform : MonoBehaviour {


	[Tooltip("Set how many times the normal jump velocity you want the player to jump with.")]
	public float velocityMultiplier = 2f;

	private PlayerController playerController;
	
	void OnTriggerEnter(Collider other)
	{
        Debug.Log("triggered");
		if (other.gameObject.CompareTag("Player"))
		{
			playerController = other.GetComponent<PlayerController>();
			playerController.PlatformJump(velocityMultiplier);
		}
	}
}
