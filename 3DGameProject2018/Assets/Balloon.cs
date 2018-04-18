using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Balloon : MonoBehaviour {

	public GameObject model;
	public ParticleSystem balloonSplashParticles;
	public ParticleSystem trailEffect;
	public Rigidbody rb;
	public PlayerController playerController;

	private ParticleLauncher particleLauncher;
	private Collider col;
	private bool isInstantiated = false, isDestroying = false;

	// Use this for initialization
	public void Instantiate () {

		if (!model)
			model = GetComponentInChildren<Animator>().gameObject;
		
		if (!balloonSplashParticles)
			Debug.LogWarning("No water splash particle system in balloon.");

		if (!rb)
			rb = GetComponent<Rigidbody>();

		if (trailEffect)
			trailEffect.Play();
			
		particleLauncher = balloonSplashParticles.GetComponent<ParticleLauncher>();
		particleLauncher.Controller = playerController;
		col = GetComponent<Collider>();
		isInstantiated = true;
	}

	void OnCollisionEnter(Collision other)
	{
		if (isInstantiated)
		{
			var otherController = other.gameObject.GetComponent<PlayerController>();
			if (otherController != null && otherController != playerController)
			{
				//Player collision. Instakill.
				otherController.TakeDamage(500, playerController);
				playerController.DealDamage();
			}
			rb.isKinematic = true;
			col.enabled = false;
			model.SetActive(false);
			trailEffect.Stop();
			balloonSplashParticles.Play();
			isDestroying = true;
		}

	}
	
	// Update is called once per frame
	void Update () {
		if (isDestroying && !balloonSplashParticles.isPlaying)
		{
			Destroy(gameObject, 1f);
		}
	}
}
