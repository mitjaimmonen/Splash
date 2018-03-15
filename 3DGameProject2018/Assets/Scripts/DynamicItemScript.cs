using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicItemScript : MonoBehaviour {

	public int health = 10;
	public float explosionForce = 1f;
	public bool isDestroyable;
	public Collider mainCollider;
	public GameObject[] ObjectsToDisableOnDestroy;

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

	}


	private void Update () {
		if (isDestroying)
		{
			
			timer += Time.deltaTime;
			mainCollider.enabled = false;
			
			if (childRigidbodies.Count > 0)
			{
				if (timer > 5f)
				{
					foreach (var child in childRigidbodies)
					{
						child.isKinematic = true;
						child.transform.position += new Vector3(0,0.5f * -Time.deltaTime,0);
					}
				}

			}
			if (timer > 15f)
				Destroy(gameObject);
		}
	}


	public void ParticleHit(Vector3 origin, Vector3 intersection)
	{
		TakeDamage(origin, intersection);
	}

	private void TakeDamage(Vector3 origin, Vector3 intersection)
	{
		currentHealth -= 1;

		if (currentHealth < 1 && isDestroyable)
		{
			isDestroying = true;
			mainRigidbody.isKinematic = true;

			foreach(var obj in ObjectsToDisableOnDestroy)
			{
				obj.SetActive(false);
			}

			if (childRigidbodies.Count > 0)
			{
				foreach (var child in childRigidbodies)
				{
					foreach(var obj in ObjectsToDisableOnDestroy)
					{
						obj.SetActive(false);
					}
					child.isKinematic = false;
					child.gameObject.SetActive(true);

					child.AddExplosionForce(Random.Range(explosionForce/2, explosionForce), transform.position,0);
				}
			}
			else if (destroyParticleSystem != null)
			{
				destroyParticleSystem.transform.position = intersection;
				destroyParticleSystem.Play();

				// mainRigidbody.gameObject.SetActive(false); //mesh renderer is attached to this.
			}


		}
		else
		{
			Vector3 dir = (transform.position - origin).normalized;
			mainRigidbody.isKinematic = false;
			mainRigidbody.AddForceAtPosition(dir/2, intersection, ForceMode.Impulse);
		}
	}
}
