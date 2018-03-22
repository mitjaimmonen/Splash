using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicItemScript : MonoBehaviour {

	public int health = 10;
	public float explosionForce = 1f;
	public bool isDestroyable, isMovable, isMovableOnDestroy, randomizeStartRotation = false;
	public Collider mainCollider;
	public GameObject[] ObjectsToDisableOnDestroy;

	[FMODUnity.EventRef] public string destroySE, takeDamageSE;

	private int currentHealth;
	private ParticleSystem destroyParticleSystem;
	private List<Rigidbody> childRigidbodies;
	private Rigidbody mainRigidbody;

	private float timer;
	private bool isDestroying;


	private void Start()
	{
		currentHealth = health;
		destroyParticleSystem = GetComponentInChildren<ParticleSystem>();
		Rigidbody[] childRB = GetComponentsInChildren<Rigidbody>(true);
		childRigidbodies = new List<Rigidbody>();
		foreach(var child in childRB)
		{
			if (child.gameObject.activeSelf)
				mainRigidbody = child;
			else
				childRigidbodies.Add(child);
		}
		if (randomizeStartRotation)
		{
			transform.rotation = Quaternion.Euler(Random.Range(0,360), Random.Range(0,360), Random.Range(0,360));
		}

	}


	private void Update () {
		if (isDestroying)
		{
			
			timer += Time.deltaTime;
			if (!isMovableOnDestroy)
				mainCollider.enabled = false;
			
			if (timer > 20f)
			{
				if (childRigidbodies.Count > 0)
				{
					foreach (var child in childRigidbodies)
					{
						child.isKinematic = true;
						child.transform.position += new Vector3(0,0.5f * -Time.deltaTime,0);
					}
				}
				else
				{
					mainRigidbody.isKinematic = true;
					transform.position += new Vector3(0,0.5f * -Time.deltaTime,0);
				}
			}
			if (timer > 30f)
				Destroy(gameObject);
		}
		if (transform.position.y < -30f)
			Destroy(gameObject);
	}


	public void ParticleHit(Vector3 origin, Vector3 intersection)
	{
		TakeDamage(origin, intersection);
	}

	private void TakeDamage(Vector3 origin, Vector3 intersection)
	{
		currentHealth -= 1;

        if (takeDamageSE != "")
		{
			Debug.Log("Playing collision sound");
			FMODUnity.RuntimeManager.PlayOneShot(takeDamageSE, transform.position);
		}

		if (currentHealth < 1 && isDestroyable && !isDestroying)
		{
			if (!isMovableOnDestroy)
				mainRigidbody.isKinematic = true;
			else 
				mainRigidbody.isKinematic = false;
			

			foreach(var obj in ObjectsToDisableOnDestroy)
			{
				obj.SetActive(false);
			}

			if (childRigidbodies.Count > 0)
			{
				foreach (var child in childRigidbodies)
				{
					child.isKinematic = false;
					child.gameObject.SetActive(true);
					child.AddExplosionForce(Random.Range(explosionForce/2, explosionForce), transform.position,0);
				}
			}
			else if (destroyParticleSystem != null)
			{
				destroyParticleSystem.transform.position = intersection;
				destroyParticleSystem.Play();
			}
			
			
			FMODUnity.RuntimeManager.PlayOneShot(destroySE, transform.position);	
			isDestroying = true;
		}
		else if (isMovable || (isDestroying && isMovableOnDestroy))
		{
			Vector3 dir = (transform.position - origin).normalized;
			dir.y += 0.5f;
			mainRigidbody.isKinematic = false;
			mainRigidbody.AddForceAtPosition(dir/2, intersection, ForceMode.Impulse);
		}
	}
}
