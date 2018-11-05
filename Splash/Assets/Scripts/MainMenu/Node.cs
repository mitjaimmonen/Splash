using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour {
    public Node backNode;
    public float backNodeSpeed;
    public Node nextNode;
    public float nextNodeSpeed;
    public GameObject focus;
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, .1f);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward);
    }
}
