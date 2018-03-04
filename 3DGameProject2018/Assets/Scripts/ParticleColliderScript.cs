using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleColliderScript : MonoBehaviour {


	private float timer = 0;

	private void Update() {
		timer += Time.deltaTime;
	}
	private void OnParticleCollision(GameObject other) {

		//Allows collisions about 10 times per second.
		//Assault rifle shoots 10 times per second, so every shot should trigger once in collision.
		if (timer > 0.09f)
		{
			Debug.Log("Particle collision");
			timer = 0;

		}

	}

}
