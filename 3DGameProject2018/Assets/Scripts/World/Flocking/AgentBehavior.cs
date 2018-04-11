using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentBehavior : MonoBehaviour {
    [HideInInspector]
    public AgentController controller;
    private Vector3 currentPosition;
    private Quaternion currentRotation;
    [HideInInspector]
    public Vector3 separation, alignment, cohesion;
    public float Velocity;
    public Animator animloop;


    private void Start()
    {
        animloop.Play("rig|rigAction", -1, Random.value);
    }

    private void Update()
    {
        currentPosition = transform.position;
        currentRotation = transform.rotation;
        float Velocity = controller.f_velocity;
        separation = Vector3.zero;
        alignment = (controller.transform.position- currentPosition).normalized;
        cohesion = controller.transform.position;
        Collider[] collisions = Physics.OverlapSphere(transform.position, controller.f_neighborDist, controller.lm_layer);
        foreach(var agent in collisions)
        {
            if(agent.gameObject != gameObject)
            {
                AgentBehavior currentagent = agent.GetComponentInParent<AgentBehavior>();
                separation += GetSeparationVector(agent.transform);
                alignment += agent.transform.forward;
                cohesion += agent.transform.position;
            }
        }
        float average;
        if(collisions.Length == 0)
        {
            average = 1.0f;
        } else
        {
            average = 1.0f / (collisions.Length);
        }
        alignment *= average;
        cohesion *= average;
        cohesion = (cohesion - currentPosition).normalized;
        Vector3 direction = separation + alignment + cohesion;
        if(direction == Vector3.zero)
        {
            direction = -transform.forward;
        }
        Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, direction.normalized);
        if(rotation != currentRotation)
        {
            transform.rotation = Quaternion.Slerp(rotation, currentRotation, Mathf.Exp(-4.0f * Time.deltaTime));
        }
        transform.position = currentPosition + transform.forward * (Velocity * Time.deltaTime);
    }


    Vector3 GetSeparationVector(Transform target)
    {
        var diff = transform.position - target.transform.position;
        var diffLen = diff.magnitude;
        var scaler = Mathf.Clamp01(1.0f - diffLen / controller.f_neighborDist);
        return diff * (scaler / diffLen);
    }
}
