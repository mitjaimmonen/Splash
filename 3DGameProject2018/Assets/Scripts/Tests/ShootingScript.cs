using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingScript : MonoBehaviour {

	public bool noScope = true;
	public Camera mainCamera;
	private ParticleSystem m_ParticleSystem;
	private FMOD_StudioEventEmitter FMODEmitter;
	private FMOD.Studio.ParameterInstance FMOD_Magazine;
	private FMOD.Studio.ParameterInstance FMOD_Shooting;
	private FMOD.Studio.EventInstance e;
	private float magazine = 100f;
		
	[FMODUnity.EventRef] public string m_EventPath;

	private Animator gunAnimator;


	// Use this for initialization
	void Awake () {
		m_ParticleSystem = gameObject.GetComponentInChildren<ParticleSystem>();
		gunAnimator = gameObject.GetComponentInChildren<Animator>();
		FMODEmitter = gameObject.GetComponent<FMOD_StudioEventEmitter>();
		e = FMODUnity.RuntimeManager.CreateInstance(m_EventPath);
		e.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform.position));
			e.getParameter("Magazine", out FMOD_Magazine);
			e.getParameter("Shooting", out FMOD_Shooting);
			
		
	}
	
	// Update is called once per frame
	void Update () {

		if (gunAnimator.GetBool("isShooting") && magazine > 0) {
			// Debug.Log(magazine);
			magazine -= Time.deltaTime * 10f;
			FMOD_Magazine.setValue(magazine);

			//If shooting from hip, randomize direction doubles
			float hipPenalty;
			if(noScope)
				hipPenalty = 2.5f;
			else
				hipPenalty = 1f;
			
			//Randomness is affected by low water amount as well
			float randomAmount = (100f - magazine) /(10000f / hipPenalty);			
			var shape = m_ParticleSystem.shape;
			shape.randomDirectionAmount = randomAmount;

			
		}
			
		

		if (Input.GetMouseButtonDown(0)) {
			FMOD_Shooting.setValue(1f);
			e.start();
			e.release();

			gunAnimator.SetBool("isShooting", true);
			magazine = 100f;
			
			m_ParticleSystem.Play();
		} 
		if (Input.GetMouseButtonUp(0) || magazine <= 0) {
			FMOD_Shooting.setValue(0);
			// e.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			m_ParticleSystem.Stop();
			gunAnimator.SetBool("isShooting", false);
		}

		float zOffset;
		float yOffset;
		float xOffset;
		if (Input.GetMouseButtonDown(1)) {
			noScope = false;
			zOffset = 1f;
			yOffset = -1f;
			xOffset = 0;
			transform.position = new Vector3(mainCamera.transform.position.x + xOffset, mainCamera.transform.position.y + yOffset ,mainCamera.transform.position.z + zOffset);
		}
		if (Input.GetMouseButtonUp(1)) {
			noScope = true;
			zOffset = 1f;
			yOffset = -1.5f;
			xOffset = 0.75f;
			transform.position = new Vector3(mainCamera.transform.position.x + xOffset, mainCamera.transform.position.y + yOffset ,mainCamera.transform.position.z + zOffset);
			
		}


	}



}
