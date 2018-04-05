using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicItemScript : MonoBehaviour, IWater {

	public int health = 10;
	public float explosionForce = 1f;
	public bool isDestroyable, isMovable, isMovableOnDestroy, randomizeStartRotation = false;
	public Collider mainCollider;
	public GameObject[] ObjectsToDisableOnDestroy;


	private int currentHealth;
	private ParticleSystem destroyParticleSystem;
	private CollisionSounds collisionSounds;
	private List<Rigidbody> childRigidbodies;
	[SerializeField] private Rigidbody mainRigidbody;

	private float timer;
	private bool isDestroying = false;



        public ParticleSplash psSplash;
        public CollisionSounds colSplashSound;

		public ParticleSplash particleSplash
        {
            get{ return psSplash;}
            set{ particleSplash = value; }
        }

        public CollisionSounds colsounds
        {
            get{ return colSplashSound;}
            set{ colsounds = value; }
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


	void Awake()
	{
		colSplashSound = GetComponent<CollisionSounds>();
		if (!collisionSounds)
			colSplashSound = GetComponentInChildren<CollisionSounds>();
		
	}
	private void Start()
	{
		currentHealth = health;
		destroyParticleSystem = GetComponentInChildren<ParticleSystem>();
		collisionSounds = GetComponent<CollisionSounds>();
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

	public void ParticleHit(Vector3 origin, Vector3 intersection)
	{
		if(health != 0)
			currentHealth -= 1;

		if (currentHealth < 1 && isDestroyable && !isDestroying)
			StartCoroutine(StartDestroy(origin, intersection));

		else if ((isMovable && !isDestroying) || (isDestroying && isMovableOnDestroy))
			Move(origin, intersection);
	}

	private void Move(Vector3 origin, Vector3 intersection)
	{
		Vector3 dir = (transform.position - origin).normalized;
		dir.y += 0.5f;
		mainRigidbody.isKinematic = false;
		mainRigidbody.AddForceAtPosition(dir/2, intersection, ForceMode.Impulse);
	}

	private IEnumerator StartDestroy(Vector3 origin, Vector3 intersection)
	{
		Debug.Log("Destroying.");
		timer = Time.time;
		isDestroying = true;
		mainRigidbody.isKinematic = isMovableOnDestroy ? false : true; //dynamic if isMovableOnDestroy
		mainCollider.enabled = isMovableOnDestroy ? true : false;
		if (collisionSounds)
			collisionSounds.PlayDestroy();
		
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
