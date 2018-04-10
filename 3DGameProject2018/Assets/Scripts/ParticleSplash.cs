using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSplash : MonoBehaviour {
	private ParticleSystem splashParticleSystem;
	private ParticleSystem.MainModule main;
	private ParticleSystem.EmitParams emitParams;
	private float splashSize, splashSpeed;




	// Use this for initialization
	void Awake()
	{
		splashParticleSystem = GetComponent<ParticleSystem>();
		emitParams = new ParticleSystem.EmitParams();

		main = splashParticleSystem.main;
		splashSize = main.startSizeMultiplier;
		splashSpeed = main.startSpeedMultiplier;

	}
	public void PlaySplash(Collider col, float splashSizeMultiplier)
	{
		Debug.Log("PlaySplash Called!");
		EmitDecalParticleAtPosition(col, splashSizeMultiplier);

	}

	void EmitDecalParticleAtPosition(Collider col, float splashSizeMultiplier)
	{
		// Vector3 rot = Vector3.up;
		// rot.z = Random.Range(0,360);
		// emitParams.velocity = Vector3.up * splashSizeMultiplier;
		splashParticleSystem.transform.position = col.transform.position;
		main.startSizeMultiplier = splashSize * splashSizeMultiplier;
		Debug.Log(main.startSizeMultiplier);
		// main.startSpeedMultiplier = splashSpeed * splashSizeMultiplier;
		Debug.Log(main.startSpeedMultiplier);

		// emitParams.rotation = Random.Range(-180f, 180f);
		// emitParams.startSize = main.startSizeMultiplier * splashSizeMultiplier;

		splashParticleSystem.Play();

	}

}
