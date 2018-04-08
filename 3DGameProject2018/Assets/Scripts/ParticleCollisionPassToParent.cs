using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleCollisionPassToParent : MonoBehaviour {

	public bool isPlayerHead;
	private CollisionBehaviour collisionBehaviour;
	private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();


	// Use this for initialization
	// void Start () {
	// 	collisionBehaviour = GetComponentInParent<CollisionBehaviour>();
	// 	if (!collisionBehaviour)
	// 		Debug.Log("No collision behaviour in parent.");

	// }
	
	// private void OnParticleCollision(GameObject other)
	// {
	// 	Debug.Log("Passing damage upwards");
	// 	ParticlePhysicsExtensions.GetCollisionEvents(other.GetComponent<ParticleSystem>(), this.gameObject, collisionEvents);
	// 	var launcher = other.GetComponent<ParticleLauncher>();
	// 	float headshotMultiplier = isPlayerHead ? launcher.HeadshotMultiplier : 1f;
	// 	if (collisionBehaviour != null)
	// 		collisionBehaviour.PlayerDamage(other, collisionEvents, headshotMultiplier);
	// }
}
