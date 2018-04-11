using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ParticleLauncher : MonoBehaviour {


	[Tooltip("Do collisions have cooldown or not.")]
	public bool ignoreTimers = false;

	private ParticleDecal particleDecal;
	private PlayerController thisPlayerController;
	private ParticleSystem thisParticleSystem;
	private List<ParticleCollisionEvent> collisionEvents;
	private float collisionCountTimer = 0, splashTimer = 0;
	private int oldLoopCount = 0;
	private int maxLoopCount = 20;


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
	public int MaxLoopCount
	{
		get{return maxLoopCount;}
		set {maxLoopCount = value;}
	}

	private void Start()
	{
		particleDecal = GameObject.Find("DecalParticles").GetComponent<ParticleDecal>();
		if (particleDecal == null)
			particleDecal = Instantiate(particleDecal, Vector3.zero,Quaternion.identity);

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

			int loopCount = Mathf.Clamp(collisionEvents.Count, 0, MaxLoopCount-oldLoopCount);

			if (loopCount > 0)
			{
				oldLoopCount += loopCount;

				if (ignoreTimers || splashTimer < Time.time - 0.025f)
				{
					for (int i = 0; i < loopCount;i++)
					{
						if (!collisionEvents[i].colliderComponent)
							continue;

						splashTimer = Time.time;		
						particleDecal.ParticleHit (collisionEvents [i]);
						if (!ignoreTimers)
							break; //We only need one decal because area of effect is not big
					}
				}
			}
		}
	}
}
