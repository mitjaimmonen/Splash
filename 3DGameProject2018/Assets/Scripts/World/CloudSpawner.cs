using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(BoxCollider))]
/********************************************
 * LastMenuController class
 *  spawns and moves clouds
 */
public class CloudSpawner : MonoBehaviour {

    public GameObject[] clouds;
    public int initialAmmount = 0;
    //cloud speed, spawn freq, and scale
    public float speed;
    public float frequencyInSeconds;
    public float cloudScale;
    //cloud drop off curve
    public float dropHeight;

    private float timer = 0;
    private List<GameObject> instantiatedClouds = new List<GameObject>();
    private BoxCollider spawnSize;

    void Start()
    {
        spawnSize = gameObject.GetComponent<BoxCollider>();
        for(int i = 0; i < initialAmmount; i++)
        {
            instantiatedClouds.Add(Instantiate(clouds[Random.Range(0, clouds.Length)], gameObject.transform));
            instantiatedClouds[instantiatedClouds.Count - 1].transform.localPosition = new Vector3(Random.Range(-spawnSize.bounds.size.x / 2, spawnSize.bounds.size.x / 2), Random.Range(-spawnSize.bounds.size.y / 2, spawnSize.bounds.size.y / 2), Random.Range(spawnSize.bounds.size.z / 2, -spawnSize.bounds.size.z / 2));
            instantiatedClouds[instantiatedClouds.Count - 1].transform.localRotation = Quaternion.identity;
            instantiatedClouds[instantiatedClouds.Count - 1].transform.localScale = new Vector3(cloudScale, cloudScale, cloudScale);
            float y = ((Mathf.PI) * (instantiatedClouds[i].transform.localPosition.z + spawnSize.bounds.size.z / 2) / spawnSize.bounds.size.z);
            float height = Mathf.Sin(y) * dropHeight;
            instantiatedClouds[instantiatedClouds.Count - 1].transform.localPosition = new Vector3(instantiatedClouds[i].transform.localPosition.x, height, instantiatedClouds[i].transform.localPosition.z + speed * Time.deltaTime);
        }
    }
    void Update()
    {
        timer+=Time.deltaTime;
        if(timer>=frequencyInSeconds)
        {
            instantiatedClouds.Add(Instantiate(clouds[Random.Range(0, clouds.Length)],gameObject.transform)) ;
            instantiatedClouds[instantiatedClouds.Count - 1].transform.localPosition = new Vector3(Random.Range(-spawnSize.bounds.size.x / 2, spawnSize.bounds.size.x / 2), Random.Range(-spawnSize.bounds.size.y / 2, spawnSize.bounds.size.y / 2), -spawnSize.bounds.size.z / 2);
            instantiatedClouds[instantiatedClouds.Count - 1].transform.localRotation = Quaternion.identity;
            instantiatedClouds[instantiatedClouds.Count - 1].transform.localScale = new Vector3(cloudScale, cloudScale, cloudScale);
            timer = 0;
        }
        for(int i = 0; i < instantiatedClouds.Count;)
        {
            float y = ((Mathf.PI) * (instantiatedClouds[i].transform.localPosition.z+ spawnSize.bounds.size.z / 2) / spawnSize.bounds.size.z);
            float height = Mathf.Sin(y)*dropHeight;
            instantiatedClouds[i].transform.localPosition = new Vector3(instantiatedClouds[i].transform.localPosition.x, height, instantiatedClouds[i].transform.localPosition.z + speed * Time.deltaTime);
            if(instantiatedClouds[i].transform.localPosition.z >= spawnSize.bounds.size.z/2)
            {
                Destroy(instantiatedClouds[i]);
                instantiatedClouds.RemoveAt(i);
            } else
            {
                i++;
            }
        }
    }
}
