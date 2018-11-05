using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupPlatform : MonoBehaviour {

	[Tooltip("Add all prefabs you want to randomize the spawn with.")]
	public GameObject[] pickups;
	public float respawnTime;
	private GameObject currentPickup;
	private float respawnTimer;

	

	void Start () {

		//Makes platforms seem more natural
		transform.rotation = Quaternion.Euler(0,  Random.Range(-180f, 180f), 0);

		if (pickups.Length == 0)
			Debug.LogWarning("No drop Prefabs to spawn!");
		else 
		{
			int index = Random.Range (0, pickups.Length);
			currentPickup = pickups[index];
		}


		currentPickup = Instantiate(currentPickup, transform.position, transform.rotation);
		currentPickup.transform.parent = gameObject.transform;

	}
	
	// Update is called once per frame
	void Update () {
		if (respawnTimer > respawnTime)
		{				
			int index = Random.Range (0, pickups.Length);
			
			currentPickup = pickups[index];

			currentPickup = Instantiate(currentPickup, transform.position, transform.rotation);
			currentPickup.transform.parent = gameObject.transform;
			respawnTimer = 0;

		}
			
		if (currentPickup == null)
			respawnTimer += Time.deltaTime;
		

	}
}
