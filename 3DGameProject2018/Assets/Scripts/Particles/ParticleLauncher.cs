using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ParticleLauncher : MonoBehaviour {


	private ParticleDecalPool decalPool;
	public GameObject damageText;


	private PlayerController thisPlayerController;
	private ParticleSystem thisParticleSystem;
	public ParticleSystem splatterParticleSystem;
	private List<ParticleCollisionEvent> collisionEvents;
	private float timer = 0;
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
		timer += Time.deltaTime;
	}
	private void OnParticleCollision(GameObject other) 
	{		
		ParticlePhysicsExtensions.GetCollisionEvents (thisParticleSystem, other, collisionEvents);
		int currentDamage = thisPlayerController.CurrentDamage;
		PlayerController otherPlayerController = other.GetComponent<PlayerController>();
		if (otherPlayerController == null)
			{
				otherPlayerController = other.GetComponentInParent<PlayerController>();				
			}
		
		for (int i = 0; i < collisionEvents.Count;i++)
		{
			if (otherPlayerController != null && collisionEvents[i].colliderComponent.gameObject.layer == LayerMask.NameToLayer("Player"))
			{
				
				if (collisionEvents[i].colliderComponent.gameObject.tag == "Head")
				{
					currentDamage = (int)(currentDamage * headshotMultiplier);
				}
				if (timer >= 0.1f)
				{
					// Debug.Log("Player Collision at world position: " + hitPoint);
					StartCoroutine(ShowDamageText(collisionEvents[i].intersection, currentDamage));
					otherPlayerController.TakeDamage(currentDamage, thisPlayerController.transform.position);
					timer = 0;
				}
			}
			else
			{
				//Particle hit environment and creates a decal.
				
				if (splatterParticleSystem != null)
					EmitAtCollisionPoint(collisionEvents[i]);
           		decalPool.ParticleHit (collisionEvents [i]);
			}
		}
	}

	private void EmitAtCollisionPoint(ParticleCollisionEvent collisionEvent)
	{
		splatterParticleSystem.transform.position = collisionEvent.intersection;


		Vector3 dir = Quaternion.LookRotation(collisionEvent.normal) * Vector3.forward;
		if (Physics.Raycast(collisionEvent.intersection + dir, -dir,2f))
		{
			splatterParticleSystem.transform.localEulerAngles = Quaternion.LookRotation(collisionEvent.normal).eulerAngles;		
		}
		else
		{
			splatterParticleSystem.transform.rotation = Quaternion.LookRotation(collisionEvent.normal);		
		}

		splatterParticleSystem.Emit(1);
	}

	IEnumerator ShowDamageText(Vector3 pos, int dmg)
	{
		// Debug.Log("Coroutine started");		
		float timer = 0, time = 0.5f;
		Vector3 directionTowards = (pos - thisPlayerController.transform.position);
		Vector3 newPos = pos;
		newPos.y += 1f;
		
		GameObject damageTextObject = Instantiate(damageText, pos, Quaternion.LookRotation(directionTowards));
		damageTextObject.layer = LayerMask.NameToLayer("Culling" + thisPlayerController.playerNumber);
		TextMesh text = damageTextObject.GetComponent<TextMesh>();
		float distance = directionTowards.magnitude;
		text.characterSize = text.characterSize * (distance/5);
		text.text = "-" +dmg + "HP";
		while (timer <= time)
		{
			Vector3 lerpPos = Vector3.Lerp(pos, newPos, timer);
			byte lerpAlpha = (byte)Mathf.Lerp(255,0, timer);
			text.color = new Color32(255,255,255,lerpAlpha);
			damageTextObject.transform.position = lerpPos;
			timer += Time.deltaTime;
			yield return new WaitForSeconds(0.03f); //about 30fps

		}
		
		// Debug.Log("Ending coroutine");
		Destroy(damageTextObject);
		yield return null;

	}


}
