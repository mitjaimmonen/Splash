﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleColliderScript : MonoBehaviour {


	public ParticleCollisionEvent particleCollider;

	private void OnParticleCollision(GameObject other) {
		Debug.Log(this);

	}

}
