using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sway : MonoBehaviour {

    public float xSize;
    public float zSize;
    public float xSpeed;
    public float zSpeed;
    public Vector3 origin;
    private Quaternion orquat;

    private void Start()
    {
        origin = transform.position;
        orquat = transform.rotation;
}
    // Update is called once per frame
    void Update () {
        float x = xSize * Mathf.Sin(xSpeed * Time.time);
        float z = zSize * Mathf.Cos(zSpeed * Time.time);
        transform.position = new Vector3(origin.x, origin.y + x, origin.z);
        transform.Rotate(new Vector3(zSize * Mathf.Sin(zSpeed * Time.time), 0, 0));
	}
}
