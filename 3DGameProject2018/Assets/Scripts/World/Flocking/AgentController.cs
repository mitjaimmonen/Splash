using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentController : MonoBehaviour {

    public GameObject go_agent;
    public float f_velocity;
    public float f_rotateSpeed;
    public float f_neighborDist;

    public LayerMask lm_layer;
    public int i_spawnCount = 1;
    public float f_flockRadius = 5;
   

    void Start()
    {
        Spawn(i_spawnCount);
    }

    private void Spawn(int count)
    {
        for(int i = 0; i < count; i++)
        {
            GameObject temp = Instantiate(go_agent, Random.insideUnitSphere * f_flockRadius, Quaternion.identity);
            temp.transform.position = transform.position;
            temp.GetComponent<AgentBehavior>().controller = this;
        }
    }
}
