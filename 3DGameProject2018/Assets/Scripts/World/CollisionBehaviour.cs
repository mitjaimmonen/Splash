using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/********************************************
 * PSCollisions
 * 
 * Script that can be attached on any map element gameObject with collider.
 * If collider gets hit by particles, script counts the hits and plays sound accordingly.
 * MaxCount defines how many collisions is allowed to be detected at once.
 * Handles water collision sounds as well.
 * Coroutine is used instead of Update to keep script faster even in case of dozens of instances in scene.
 */

[RequireComponent(typeof(SoundBehaviour))]
public class CollisionBehaviour : MonoBehaviour {

	public bool isPlayer = false;
	public int maxCount = 5;

	private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
	private DynamicItemScript dynamicItem;
	private PlayerController playerController;
	public SoundBehaviour soundBehaviour;

	private bool isHead = false;
	private float headshotMultiplier = 1f, stackedDamage = 0;
	private int defaultDamage;
	private int oldCount = 0, count = 0;

	private float collisionCountTimer;

	void Start()
	{
		dynamicItem = GetComponentInChildren<DynamicItemScript>(); //Also checks this.gameObject
		if (dynamicItem == null)
			dynamicItem = GetComponentInParent<DynamicItemScript>();
		
		playerController = GetComponentInParent<PlayerController>(); //Also checks this.gameObject
		if (playerController != null)
		{
			isPlayer = true;
			maxCount = playerController.CurrentWeapon.weaponData.maxCollisionCountPerFrame;
		}
		
		soundBehaviour = GetComponent<SoundBehaviour>();

	}


	private void OnParticleCollision(GameObject other)
	{
		if (collisionCountTimer < Time.time - 0.1f)
		{
			oldCount = 0;
			collisionCountTimer= Time.time;
		}
		ParticlePhysicsExtensions.GetCollisionEvents (other.GetComponent<ParticleSystem>(), this.gameObject, collisionEvents);
		count = Mathf.Clamp(collisionEvents.Count, 0, maxCount-oldCount);


		if (count > 0)
		{
			oldCount += count;
			
			Vector3 intersection = collisionEvents[0].intersection;
			soundBehaviour.PlayCollisionSound(count, intersection);

			if (dynamicItem != null)
				dynamicItem.ParticleHit(other.transform.position, intersection, count);
			
			else if (isPlayer)
			{
				ParticleLauncher otherParticleLauncher = other.GetComponent<ParticleLauncher>();
				PlayerController otherPlayerController = null;

				if (otherParticleLauncher)
				{
					otherPlayerController = otherParticleLauncher.Controller;
					if (!otherPlayerController)
						Debug.LogWarning("Playercontroller not found!");
				}
				else
					Debug.LogWarning("ParticleLauncher not found!");

				if (playerController != null && otherPlayerController != null && playerController != otherPlayerController)
					PlayerDamage(otherPlayerController, otherParticleLauncher, collisionEvents);
			}

			
		}
	}

	public void PlayerDamage(PlayerController attacker, ParticleLauncher attackerParticles,List<ParticleCollisionEvent> collisionEvents)
	{

		stackedDamage = 0;
		defaultDamage = attacker.CurrentDamage;
		
		for(int i = 0; i < count; i++)
		{
			if (collisionEvents[i].colliderComponent == null)
			{
				count = Mathf.Min(count++, maxCount*2); //In case of endless loop which shouldnt happen in the first place
				continue;
			}
			isHead = collisionEvents[i].colliderComponent.gameObject.CompareTag("Head");
			headshotMultiplier = isHead ? attackerParticles.HeadshotMultiplier : 1f;
			stackedDamage += defaultDamage * headshotMultiplier;

		}

		attacker.DealDamage();
		Debug.Log(stackedDamage);
		playerController.TakeDamage((int)stackedDamage, attacker);
				
	}
}
