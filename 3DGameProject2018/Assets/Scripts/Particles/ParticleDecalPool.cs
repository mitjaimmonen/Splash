using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleDecalPool : MonoBehaviour {

	public int maxDecals = 1000;
	public float decalSizeMin = 0.5f, decalSizeMax = 1.5f;
	public Gradient colorGradient;


	private int particleDecalDataIndex;
	private ParticleSystem decalParticleSystem;
	private ParticleDecalData[] particleData;
	private ParticleSystem.Particle[] particles;
	
	


	// Use this for initialization
	void Start () {
		decalParticleSystem = GetComponent<ParticleSystem>();
		particles = new ParticleSystem.Particle[maxDecals];
		particleData = new ParticleDecalData[maxDecals];
		for (int i = 0; i < maxDecals; i++)
		{
			particleData[i] = new ParticleDecalData();
		}
	}
	
	public void ParticleHit(ParticleCollisionEvent colEvent)
	{
		SetParticleData(colEvent,colorGradient);
		DisplayParticles();
	}

	void SetParticleData(ParticleCollisionEvent particleCollisionEvent, Gradient colorGradient)
	{
		if (particleDecalDataIndex >= maxDecals)
		{
			particleDecalDataIndex = 0;
		}
		Vector3 rotationVector = 
		
		particleData[particleDecalDataIndex].position = particleCollisionEvent.intersection;


		Vector3 particleRotationEuler = Quaternion.LookRotation(particleCollisionEvent.normal).eulerAngles;
		Vector3 dir = Quaternion.LookRotation(particleCollisionEvent.normal) * Vector3.forward;
		if (Physics.Raycast(particleData[particleDecalDataIndex].position+dir, -dir,2f))
		{
			// Debug.DrawRay(particleData[particleDecalDataIndex].position +dir, -dir, Color.red, 1f);
			particleRotationEuler = Quaternion.LookRotation(-particleCollisionEvent.normal).eulerAngles;
		}
		particleRotationEuler.z = Random.Range(0,360);


		particleData[particleDecalDataIndex].rotation = particleRotationEuler;

		particleData[particleDecalDataIndex].size = Random.Range(decalSizeMin, decalSizeMax);

		particleData[particleDecalDataIndex].color = colorGradient.Evaluate (Random.Range(0f, 1f));

		particleDecalDataIndex++;

	}

	void DisplayParticles()
	{
		for(int i = 0; i < particleData.Length; i++)
		{
			particles[i].position = particleData[i].position;
			particles[i].rotation3D = particleData[i].rotation;
			particles[i].startSize = particleData[i].size;
			particles[i].startColor = particleData[i].color;
			
		}
		decalParticleSystem.SetParticles(particles, particles.Length);
	}
}
