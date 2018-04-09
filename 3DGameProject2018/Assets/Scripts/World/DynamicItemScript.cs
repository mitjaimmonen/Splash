using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CollisionBehaviour))]
public class DynamicItemScript : MonoBehaviour, IWater {

	public int health = 10;
	public float explosionForce = 1f;
	public bool isDestroyable, isMovable, isMovableOnDestroy, randomizeStartRotation = false;
	public Collider mainCollider;
	public GameObject[] ObjectsToDisableOnDestroy;


	private int currentHealth;
	private ParticleSystem destroyParticleSystem;
	private List<Rigidbody> childRigidbodies;
	[SerializeField] private Rigidbody mainRigidbody;

	private List<ParticleCollisionEvent> collisionEvents;

	private float timer;
	private bool isDestroying = false;

	#region IWater implementation
		public ParticleSplash psSplash;
		public CollisionBehaviour collisionBehaviour;

		public ParticleSplash ParticleSplash
		{
			get{ return psSplash;}
			set{ ParticleSplash = value; }
		}

		public CollisionBehaviour ColBehaviour
		{
			get{ return collisionBehaviour;}
			set{ ColBehaviour = value; }
		}

		public float psSplashSizeMultiplier = 1;
		public float splashSizeMultiplier
		{
			get{ return psSplashSizeMultiplier; }
			set{ splashSizeMultiplier = value; }   
		}
		public void WaterInteraction(){
			Destroy(gameObject, 1f);
		}

	#endregion


	void Awake()
	{
		collisionEvents = new List<ParticleCollisionEvent>();
		collisionBehaviour = GetComponent<CollisionBehaviour>();
		if (!collisionBehaviour)
			collisionBehaviour = GetComponentInChildren<CollisionBehaviour>();
		
	}
	private void Start()
	{
		currentHealth = health;
		destroyParticleSystem = GetComponentInChildren<ParticleSystem>();
		Rigidbody[] childRB = GetComponentsInChildren<Rigidbody>(true);
		childRigidbodies = new List<Rigidbody>();
		foreach(var child in childRB)
		{
			if (child.gameObject.activeSelf && !mainRigidbody)
				mainRigidbody = child;
			else if (child == mainRigidbody)
				continue;
			else
				childRigidbodies.Add(child);
		}
		if (randomizeStartRotation)
		{
			transform.rotation = Quaternion.Euler(Random.Range(0,360), Random.Range(0,360), Random.Range(0,360));
		}

	}


	public void ParticleHit(Vector3 origin, Vector3 intersection, int damage)
	{
		

		if(health > 0)
			currentHealth -= damage;

		if (currentHealth < 1 && isDestroyable && !isDestroying)
			StartCoroutine(StartDestroy(origin, intersection));

		else if ((isMovable && !isDestroying) || (isDestroying && isMovableOnDestroy))
			Move(origin, intersection, damage);
	}

	private void Move(Vector3 origin, Vector3 intersection, int damage)
	{
		Debug.Log("move");
		Vector3 dir = (transform.position - origin).normalized * damage;
		dir.y += 0.5f * damage;
		mainRigidbody.isKinematic = false;
		mainRigidbody.AddForceAtPosition(dir, intersection, ForceMode.Impulse);
	}

	private IEnumerator StartDestroy(Vector3 origin, Vector3 intersection)
	{
		Debug.Log("Destroying.");
		timer = Time.time;
		isDestroying = true;
		mainRigidbody.isKinematic = isMovableOnDestroy ? false : true; //dynamic if isMovableOnDestroy
		mainCollider.enabled = isMovableOnDestroy ? true : false;
		if (collisionBehaviour)
			collisionBehaviour.soundBehaviour.PlayDestroy(transform.position);
		
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

		foreach(var obj in ObjectsToDisableOnDestroy)
		{
			obj.SetActive(false);
		}


		yield return new WaitForEndOfFrame();
		while (isDestroying)
		{
			if (timer < Time.time - 30f)
				Destroy(gameObject);

			else if (timer < Time.time - 20f)
			{
				if (childRigidbodies.Count > 0)
				{
					foreach (var child in childRigidbodies)
					{
						child.isKinematic = true;
						child.transform.position += new Vector3(0,-0.01f,0);
					}
				}
				else
				{
					mainRigidbody.isKinematic = true;
					transform.position += new Vector3(0,-0.005f,0);
				}
			}
			yield return new WaitForSeconds(0.1f);
		}
		yield break;
	}



}
