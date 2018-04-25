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

 public enum CollisionParticles
 {
	 None = 0,
	 LeafParticles = 1
 }

[RequireComponent(typeof(SoundBehaviour))]
public class CollisionBehaviour : MonoBehaviour {

	public bool isPlayer = false, allowMaxCountOverrideByWeapons = false;
	public int maxCount = 5;
	public CollisionParticles collisionParticles;

	public SoundBehaviour soundBehaviour;
	private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
	private DynamicItemScript dynamicItem;
	private PlayerController playerController;
	private ParticleSystem hitParticles;


	private float hitParticlesSize;
	private bool isHead = false, hitmarkerPlayed = false;
	private float headshotMultiplier = 1f;
	private float defaultDamage = 1, stackedDamage = 1;
	private int oldCount = 0, count = 0, tempCount = 0;

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
		}

		switch (collisionParticles) 
		{
			case CollisionParticles.LeafParticles:
				hitParticles = GameObject.Find("LeafParticles").GetComponent<ParticleSystem>();
			break;
		}
		
		soundBehaviour = GetComponent<SoundBehaviour>();

	}


	private void OnParticleCollision(GameObject other)
	{		
		ParticleLauncher otherParticleLauncher = other.GetComponentInChildren<ParticleLauncher>();
		PlayerController otherPlayerController = null;

		if (otherParticleLauncher)
			otherPlayerController = otherParticleLauncher.Controller;
		else
			return;

		if (allowMaxCountOverrideByWeapons)
			maxCount = otherPlayerController.CurrentWeapon.weaponData.maxCollisionCount;


		if (collisionCountTimer < Time.time - 0.033f) //30tps
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
			tempCount = Mathf.Max(5, maxCount); //Sound works better with bigger maxCount
			soundBehaviour.PlayCollisionSound(count/tempCount, intersection);

			if (hitParticles != null)
				PlayParticleEffect(intersection);

			if (dynamicItem != null)
				dynamicItem.ParticleHit(other.transform.position, intersection, (int)(count * otherPlayerController.CurrentDamage));
			
			else if (isPlayer && playerController != otherPlayerController)
			{
				PlayerDamage(otherPlayerController, otherParticleLauncher, collisionEvents);
			}

			
		}
	}

	private void PlayParticleEffect(Vector3 intersection)
	{
		hitParticles.transform.position = intersection;
		hitParticles.Play();
	}

	public void PlayerDamage(PlayerController attacker, ParticleLauncher attackerParticles,List<ParticleCollisionEvent> collisionEvents)
	{

		stackedDamage = 0;
		defaultDamage = attacker.CurrentDamage;
		
		for(int i = 0; i < count; i++)
		{
			if (collisionEvents[i].colliderComponent == null)
			{
				count = i;
				Debug.Log(collisionEvents[i]);
				break;
			}
			isHead = collisionEvents[i].colliderComponent.gameObject.CompareTag("Head");
			headshotMultiplier = isHead ? attackerParticles.HeadshotMultiplier : 1f;
			stackedDamage += defaultDamage * headshotMultiplier;
			Debug.Log("Stacked damage: " + stackedDamage);

		}
		Debug.Log("attacker particles: " + attackerParticles);
		Debug.Log("Stacked damage: " + stackedDamage + ", count: " + count);
		
		soundBehaviour.PlayHitmarker(attacker, isHead);
		attacker.DealDamage();
		playerController.TakeDamage((int)stackedDamage, attacker);
		hitmarkerPlayed = false;
				
	}
}
