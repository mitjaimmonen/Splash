using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleColliderScript : MonoBehaviour {


	private float timer = 0;
	private PlayerController thisPlayerController;


	private void Awake() 
	{
		thisPlayerController = GetComponentInParent<PlayerController>();
	}

	private void Update() 
	{
		timer += Time.deltaTime;
	}
	private void OnParticleCollision(GameObject other) 
	{
		
		if (other.layer == 9 && other != thisPlayerController.gameObject && timer > 0.09f)
		{
			timer = 0;
			Debug.Log("Particle collision with another player.");
			// thisPlayer.HitOtherPlayer(other);
			PlayerController otherPlayerController = other.GetComponent<PlayerController>();
			otherPlayerController.TakeDamage(thisPlayerController.currentDamage);


		}

		//Allows collisions about 10 times per second.
		//Assault rifle shoots 10 times per second, so every shot should trigger once in collision.


	}

}
