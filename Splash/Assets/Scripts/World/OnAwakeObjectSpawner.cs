using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/********************************************
 * OnAwakeObjectSpawner
 *
 *
 * Spawns specified objects with spawner's scale and rotation.
 * Spawner destroys after doing its job unless bool is set false or spawner is parent of the object it spawned.
 * Can be used in multiple places, e.g. fruit stand spawns a prefab which spawns all fruits under itself.
 * 
 * 
 * 
 */

public class OnAwakeObjectSpawner : MonoBehaviour {

	public Vector3 positionOffset;
	public GameObject objectToSpawn;
	public GameObject parent;
	public bool destroyAfterSpawn;

	// Use this for initialization
	void Awake () {
		GameObject obj;
		obj = Instantiate(objectToSpawn, transform.position + positionOffset, transform.localRotation);
		obj.transform.localScale = transform.localScale;
		obj.tag = gameObject.tag;
		obj.layer = gameObject.layer;

		if (parent == null)
			obj.transform.parent = gameObject.transform;
		else
			obj.transform.parent = parent.transform;

		if (destroyAfterSpawn && obj.transform.parent != gameObject.transform)
			Destroy(gameObject); //Spawner not needed anymore.
		
	}

}
