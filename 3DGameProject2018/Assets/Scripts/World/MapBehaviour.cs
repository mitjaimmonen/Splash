using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapBehaviour : MonoBehaviour {

	public GameObject[] objectsToEnableOnAwake;
	void Start () {
		foreach (var obj in objectsToEnableOnAwake)
		{
			obj.SetActive(true);
		}
	}
}
