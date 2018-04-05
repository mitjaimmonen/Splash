using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSplash : MonoBehaviour {
	private ParticleSystem splashParticleSystem;
	private ParticleSystem.MainModule main;
	private ParticleSystem.EmitParams emitParams;
	private float splashSize;
	
	public Gradient colorGradient;



	// Use this for initialization
	void Awake()
	{
		splashParticleSystem = GetComponent<ParticleSystem>();
		emitParams = new ParticleSystem.EmitParams();

		main = splashParticleSystem.main;
		splashSize = main.startSizeMultiplier;
		
	}
	public void PlaySplash(Collider col, float splashSizeMultiplier)
	{
		EmitDecalParticleAtPosition(col, splashSizeMultiplier, colorGradient);

	}

	void EmitDecalParticleAtPosition(Collider col, float splashSizeMultiplier, Gradient colorGradient)
	{
		Vector3 rot = Vector3.up;
		rot.z = Random.Range(0,360);
		emitParams.rotation3D = rot;
		emitParams.startColor = colorGradient.Evaluate(Random.Range(0f,1f));
		emitParams.position = col.transform.position;
		emitParams.startSize *= splashSizeMultiplier;

		splashParticleSystem.Emit(emitParams, 1);
	}
	
}
