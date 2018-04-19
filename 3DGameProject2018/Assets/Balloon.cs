using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Balloon : MonoBehaviour {

	public GameObject model;
	public ParticleSystem balloonSplashParticles;
	public ParticleSystem trailEffect;
	public Rigidbody rb;
	public PlayerController playerController;
	public float destroyTime = 5f;
	[FMODUnity.EventRef] public string balloonSplashSE;

	public ParticleLauncher particleLauncher;
	private Collider col;
	private bool isInstantiated = false, isDestroying = false;
	private float destroyTimer;

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
			
		particleLauncher.Controller = playerController;
		col = GetComponent<Collider>();
		isInstantiated = true;
	}

	void OnCollisionEnter(Collision other)
	{
		if (isInstantiated)
		{
			BlowUp(other);
		}

	}

	private void BlowUp(Collision other)
	{
		if (other != null)
		{
			var otherController = other.gameObject.GetComponent<PlayerController>();
			if (otherController != null && otherController != playerController)
			{
				//Player collision. Instakill.
				otherController.TakeDamage(500, playerController);
				playerController.DealDamage();
			}
		}

		rb.isKinematic = true;
		col.enabled = false;
		model.SetActive(false);
		FMODUnity.RuntimeManager.PlayOneShot(balloonSplashSE, transform.position);
		trailEffect.Stop();
		balloonSplashParticles.Play();
		isDestroying = true;
	}
	
	// Update is called once per frame
	void Update () {

		if (isInstantiated)
		{
			destroyTimer += Time.deltaTime;

			if (destroyTimer > destroyTime && !isDestroying)
				BlowUp(null);

			if (isDestroying && !balloonSplashParticles.isPlaying)
			{
				Destroy(gameObject, 1f);
			}
		}

	}
}
