using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Balloon : MonoBehaviour {

	public GameObject model;
	public ParticleSystem balloonSplashParticles;
	public ParticleSystem trailEffect;
	public Rigidbody rb;
	public PlayerController playerController;
	public LayerMask mask;
	public float explosionCheckRadius = 5;
	public float destroyTime = 5f;
	[FMODUnity.EventRef] public string balloonSplashSE;

	public ParticleLauncher particleLauncherParent, particleLauncherChild;
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
			
		particleLauncherParent.Controller = playerController;
		particleLauncherChild.Controller = playerController;
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
		rb.isKinematic = true;
		col.enabled = false;
		model.SetActive(false);
		FMODUnity.RuntimeManager.PlayOneShot(balloonSplashSE, transform.position);
		trailEffect.Stop();
		balloonSplashParticles.Play();
		isDestroying = true;


		List<GameObject> blastedPlayers = new List<GameObject>();
		Collider[] objectsInRange = Physics.OverlapSphere(transform.position, explosionCheckRadius, mask);
		
		foreach(var col in objectsInRange)
		{
			Vector3 direction = col.transform.position - transform.position;
			float magnitude = direction.magnitude;
			direction = direction.normalized;
			RaycastHit hit;
			Physics.Raycast(transform.position + direction/5f, col.transform.position - transform.position, out hit, magnitude);
			// Debug.Log(!blastedPlayers.Contains(col.gameObject) + ", " + hit.collider + ", " + col + ", "+ col.gameObject.tag);
			if (!blastedPlayers.Contains(col.gameObject) && col.gameObject.tag == "Torso")
			{
				// Debug.Log("magnitude in balloon blast: "+magnitude + ", direction:" + direction);
				blastedPlayers.Add(col.gameObject);
				//ROCKET JUMP/KNOCKBACK CODE HERE
			}
		}


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
