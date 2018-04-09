using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ParticleLauncher : MonoBehaviour {

	public int maxLoopCount = 20;
	public bool ignoreTimers = false;

	private ParticleDecal particleDecal;
	private PlayerController thisPlayerController;
	private ParticleSystem thisParticleSystem;
	private ParticleSystem splatterParticleSystem;
	private List<ParticleCollisionEvent> collisionEvents;
	private float collisionCountTimer = 0, splashTimer = 0;
	private int oldLoopCount = 0;


	private float headshotMultiplier;
	public float HeadshotMultiplier
	{
		get {return headshotMultiplier;}
		set {headshotMultiplier = value;}
	}

	public PlayerController Controller
	{
		get {return thisPlayerController;}
	}

	private void Start()
	{
		particleDecal = GameObject.Find("DecalParticles").GetComponent<ParticleDecal>();
		if (particleDecal == null)
			particleDecal = Instantiate(particleDecal, Vector3.zero,Quaternion.identity);

		splatterParticleSystem = GameObject.Find("SplatterParticles").GetComponent<ParticleSystem>();
		thisPlayerController = GetComponentInParent<PlayerController>();
		thisParticleSystem = GetComponent<ParticleSystem>();
		collisionEvents = new List<ParticleCollisionEvent>();
	}

	private void OnParticleCollision(GameObject other)
	{
		if (other.layer == LayerMask.NameToLayer("Environment"))
		{
			if (collisionCountTimer < Time.time - 0.1f)
			{
				oldLoopCount = 0;
				collisionCountTimer= Time.time;
			}

			ParticlePhysicsExtensions.GetCollisionEvents (thisParticleSystem, other, collisionEvents);

			int loopCount = Mathf.Clamp(collisionEvents.Count, 0, maxLoopCount-oldLoopCount);

			if (loopCount > 0)
			{
				oldLoopCount += loopCount;

				if (ignoreTimers)
				{
					for (int i = 0; i < loopCount;i++)
					{
						if (!collisionEvents[i].colliderComponent)
							continue;

						splashTimer = Time.time;		
						particleDecal.ParticleHit (collisionEvents [i]);
						if (splatterParticleSystem != null)
							EmitSplashAtCollisionPoint(collisionEvents[i]);
					}
				}
				else if (splashTimer < Time.time - 0.025f && collisionEvents[0].colliderComponent )
				{
					splashTimer = Time.time;		
					particleDecal.ParticleHit (collisionEvents[0]);
					if (splatterParticleSystem != null)
						EmitSplashAtCollisionPoint(collisionEvents[0]);
				}
			}
		}

	}

	private void EmitSplashAtCollisionPoint(ParticleCollisionEvent collisionEvent)
	{
		splatterParticleSystem.transform.position = collisionEvent.intersection;
		splatterParticleSystem.transform.localEulerAngles = Quaternion.LookRotation(collisionEvent.normal).eulerAngles;

		splatterParticleSystem.Emit(1);
	}


}
