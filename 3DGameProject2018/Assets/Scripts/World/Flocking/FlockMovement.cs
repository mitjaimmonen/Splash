using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockMovement : MonoBehaviour {

    public float xSize;
    public float zSize;
    public float xSpeed;
    public float zSpeed;
    public GameObject origin;
    // Update is called once per frame
    private void Start()
    {
        float x = xSize * Mathf.Sin(xSpeed * Time.time);
        float z = zSize * Mathf.Cos(zSpeed * Time.time);
        transform.position = origin.transform.position + new Vector3(x, 0, z);
    }
    void Update () {
        float x = xSize * Mathf.Sin(xSpeed * Time.time);
        float z = zSize * Mathf.Cos(zSpeed * Time.time);
        transform.position = origin.transform.position + new Vector3(x, 0, z);
    }
}
