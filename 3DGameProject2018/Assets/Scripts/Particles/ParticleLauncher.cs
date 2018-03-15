using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ParticleLauncher : MonoBehaviour {


	private ParticleDecalPool decalPool;
	private PlayerController thisPlayerController;
	private ParticleSystem thisParticleSystem;
	public ParticleSystem splatterParticleSystem;
	private List<ParticleCollisionEvent> collisionEvents;
	private float collisionCountTimer = 0, damageTimer = 0;
	private int maxLoopCount = 20, oldLoopCount = 0;
	public float headshotMultiplier;



	private void Awake()
	{
		decalPool = GameObject.Find("DecalParticles").GetComponent<ParticleDecalPool>();
		if (decalPool == null)
			decalPool = Instantiate(decalPool, Vector3.zero,Quaternion.identity);

		splatterParticleSystem = GameObject.Find("SplatterParticles").GetComponent<ParticleSystem>();
		thisPlayerController = GetComponentInParent<PlayerController>();
		thisParticleSystem = GetComponent<ParticleSystem>();
		collisionEvents = new List<ParticleCollisionEvent>();
	}

	private void Update()
	{
		collisionCountTimer += Time.deltaTime;
		damageTimer += Time.deltaTime;
	}
	private void OnParticleCollision(GameObject other)
	{
		if (collisionCountTimer >= 0.1f)
		{
			//This allows to have maxLoopCount amount of collisions independently from framerate.
			//Without oldLoopCount, the first time collisions are called, the list might not be full yet,
			//but would be in next frame and timer wouldnt allow more collision calls.
			Debug.Log("LoopCount reset");
			oldLoopCount = 0;
			collisionCountTimer=0;
		}

		ParticlePhysicsExtensions.GetCollisionEvents (thisParticleSystem, other, collisionEvents);

		int loopCount = Mathf.Clamp(collisionEvents.Count, 0, maxLoopCount-oldLoopCount);
		// Debug.Log("CollisionEvents count: " + collisionEvents.Count + ",  " + "Loop count: " + loopCount + ",  " + "Old Loop count: " + oldLoopCount);

		if (loopCount > 0)
		{
			oldLoopCount += loopCount;
			PlayerController otherPlayerController = null;
			int currentDamage = thisPlayerController.CurrentDamage;

			for (int i = 0; i < loopCount;i++)
			{
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


						if (thisPlayerController.currentWeapon.gameObject.name == "AutoRifle" && damageTimer > 0.09f)
						{
							Debug.Log("AutoRifle hit player");
							thisPlayerController.DealDamage();
							otherPlayerController.TakeDamage(currentDamage, thisPlayerController.transform.position);
							damageTimer = 0;
							
						}
						else if (thisPlayerController.currentWeapon.gameObject.name == "Shotgun")
						{
							Debug.Log("Shotgun hit player");
							thisPlayerController.DealDamage();
							otherPlayerController.TakeDamage(currentDamage, thisPlayerController.transform.position);
						}
					}
				}
				else
				{
					decalPool.ParticleHit (collisionEvents [i]);
					if (splatterParticleSystem != null)
						EmitSplashAtCollisionPoint(collisionEvents[i]);
					
					if (collisionEvents[i].colliderComponent.gameObject.tag == "Destroyable" )
					{
						DynamicItemScript script = collisionEvents[i].colliderComponent.GetComponentInParent<DynamicItemScript>();
						if (script != null)
						{
							Debug.Log("Collided with dynamic item.");
							script.ParticleHit(thisPlayerController.transform.position, collisionEvents[i].intersection);
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
