using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleDecal : MonoBehaviour {
	public float decalSizeMin = 0.5f, decalSizeMax = 1.5f;
	public Gradient colorGradient;


	private int particleDecalDataIndex;
	private ParticleSystem decalParticleSystem;
	private ParticleSystem.EmitParams emitParams;


	// Use this for initialization
	void Start () {
		decalParticleSystem = GetComponent<ParticleSystem>();
		emitParams = new ParticleSystem.EmitParams();
	}
	
	public void ParticleHit(ParticleCollisionEvent colEvent)
	{
		Debug.Log("creating decal");
		EmitDecalParticleAtPosition(colEvent, colorGradient);
	}

	void EmitDecalParticleAtPosition(ParticleCollisionEvent particleCollisionEvent, Gradient colorGradient)
	{
		
		emitParams.rotation3D = Quaternion.LookRotation(-particleCollisionEvent.normal).eulerAngles;
		Vector3 rot = emitParams.rotation3D;
		rot.z = Random.Range(0,360);
		emitParams.rotation3D = rot;
		emitParams.startColor = colorGradient.Evaluate(Random.Range(0f,1f));
		emitParams.position = particleCollisionEvent.intersection;
		emitParams.startSize = Random.Range(decalSizeMin, decalSizeMax);

		decalParticleSystem.Emit(emitParams, 1);
	}
}



