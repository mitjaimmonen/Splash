using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsKeepAwake : MonoBehaviour {

	Rigidbody rb;
	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody>();
		rb.sleepThreshold = 0;
	}
	
}
