using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingScript : MonoBehaviour {

	private ParticleSystem m_ParticleSystem;
	private Animator gunAnimator;


	// Use this for initialization
	void Awake () {
		m_ParticleSystem = gameObject.GetComponentInChildren<ParticleSystem>();
		gunAnimator = gameObject.GetComponentInChildren<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0)) {
			gunAnimator.SetBool("isShooting", true);
			
			m_ParticleSystem.Play();
		} else if (Input.GetMouseButtonUp(0)) {
			gunAnimator.SetBool("isShooting", false);
			
			m_ParticleSystem.Stop();

		}

		if (Input.GetMouseButtonDown(1))
		{
			m_ParticleSystem.Play();
		}
	}
}
