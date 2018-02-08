using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingScript : MonoBehaviour {

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
			Debug.Log(magazine);
			magazine -= Time.deltaTime * 10;
			FMOD_Magazine.setValue(magazine);
		}
			
		

		if (Input.GetMouseButtonDown(0)) {
			gunAnimator.SetBool("isShooting", true);
			magazine = 100f;
			
			FMOD_Shooting.setValue(1f);

			e.start();
			e.release();
			
			m_ParticleSystem.Play();
		} 
		if (Input.GetMouseButtonUp(0) || magazine <= 0) {
			gunAnimator.SetBool("isShooting", false);
				
			FMOD_Shooting.setValue(0);
			// e.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			m_ParticleSystem.Stop();

		}


	}



}
