using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ParticleLauncher : MonoBehaviour {

	public float damageInterval = 0.09f;
	public int maxLoopCount = 20;
	public bool ignoreTimers = false;

	private ParticleDecal particleDecal;
	private PlayerController thisPlayerController;
	private ParticleSystem thisParticleSystem;
	private ParticleSystem splatterParticleSystem;
	private List<ParticleCollisionEvent> collisionEvents;
	private float collisionCountTimer = 0, damageTimer = 0, splashTimer = 0, clutterHitTimer = 0;
	private int oldLoopCount = 0;
	private float headshotMultiplier;

	public float HeadshotMultiplier
	{
		set {headshotMultiplier = value;}
	}



	private void Awake()
	{
		particleDecal = GameObject.Find("DecalParticles").GetComponent<ParticleDecal>();
		if (particleDecal == null)
			particleDecal = Instantiate(particleDecal, Vector3.zero,Quaternion.identity);

		splatterParticleSystem = GameObject.Find("SplatterParticles").GetComponent<ParticleSystem>();
		thisPlayerController = GetComponentInParent<PlayerController>();
		thisParticleSystem = GetComponent<ParticleSystem>();
		collisionEvents = new List<ParticleCollisionEvent>();
	}

	private void Update()
	{
		collisionCountTimer += Time.deltaTime;
		splashTimer += Time.deltaTime;
		damageTimer += Time.deltaTime;
		clutterHitTimer += Time.deltaTime;
	}
	private void OnParticleCollision(GameObject other)
	{
		if (collisionCountTimer >= 0.1f)
		{
			//This allows to have maxLoopCount amount of collisions independently from framerate.
			//Without oldLoopCount, the first time collisions are called, the list might not be full yet,
			//but would be in next frame and timer wouldnt allow more collision calls.
			oldLoopCount = 0;
			collisionCountTimer=0;
		}

		ParticlePhysicsExtensions.GetCollisionEvents (thisParticleSystem, other, collisionEvents);

		int loopCount = Mathf.Clamp(collisionEvents.Count, 0, maxLoopCount-oldLoopCount);

		if (loopCount > 0)
		{
			oldLoopCount += loopCount;
			PlayerController otherPlayerController = null;
			int currentDamage = thisPlayerController.CurrentDamage;

			for (int i = 0; i < loopCount;i++)
			{
				if (!collisionEvents[i].colliderComponent)
				{
					//Collided multiple objects during same frame.
					continue;
				}
				if (collisionEvents[i].colliderComponent.gameObject.layer == LayerMask.NameToLayer("Player"))
				{
					if (otherPlayerController == null)
						{
							otherPlayerController = other.GetComponent<PlayerController>();
							if (otherPlayerController == null)
								otherPlayerController = other.GetComponentInParent<PlayerController>();
						}

					if (otherPlayerController != thisPlayerController)
					{

						if (collisionEvents[i].colliderComponent.gameObject.tag == "Head")
						{
							Debug.Log("Headshot");
							currentDamage = (int)(currentDamage * headshotMultiplier);
						}


						if (ignoreTimers || damageTimer > 0.09f)
						{
							Debug.Log("Particle hit player");
							thisPlayerController.DealDamage();
							otherPlayerController.TakeDamage(currentDamage, thisPlayerController.transform.position, thisPlayerController);
							damageTimer = 0;
							
						}
					}
				}
				else if (splashTimer > 0.025f || ignoreTimers )
				{
					splashTimer = 0;		
					particleDecal.ParticleHit (collisionEvents [i]);
					Debug.Log(splashTimer + collisionEvents[i].colliderComponent.gameObject.name);
					if (splatterParticleSystem != null)
						EmitSplashAtCollisionPoint(collisionEvents[i]);
				}
				else if (clutterHitTimer > 0.09f || ignoreTimers)
				{
					clutterHitTimer = 0;
					if (collisionEvents[i].colliderComponent.gameObject.tag == "Dynamic Elements" )
					{
						DynamicItemScript script = collisionEvents[i].colliderComponent.GetComponentInParent<DynamicItemScript>();
						if (script != null)
						{
							Debug.Log("Collided with dynamic item.");
							script.ParticleHit(thisPlayerController.transform.position, collisionEvents[i].intersection);
						}
					}
					else if (collisionEvents[i].colliderComponent.gameObject.tag == "Static Elements")
					{
						if (collisionEvents[i].colliderComponent.gameObject.name == "LampLight")
						{
							BreakableLampScript script = collisionEvents[i].colliderComponent.GetComponent<BreakableLampScript>();
							if (script != null)
								script.TakeDamage();
						}
					}
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
