using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FromToNode : MonoBehaviour {

    public MeinMenuHandler handler;
    private bool moving = false;
    
    public bool StartTransition(Node node, float speed)
    {
        if(!moving)
        {
            StartCoroutine(Move(node, speed));
            return true;
        }
        return false;
    }

    IEnumerator Move(Node node, float speed)
    {
        moving = true;
        float time = 0;
        Vector3 initPos = transform.position;
        Quaternion initRot = transform.rotation;
        while(transform.position != node.transform.position)
        {
            transform.position = Vector3.Lerp(initPos, node.transform.position, time / speed);
            transform.rotation = Quaternion.Slerp(initRot, node.transform.rotation, time / speed);
            yield return null;
            time += Time.deltaTime;
        }
        moving = false;
        handler.currentNode = node;
        handler.SetFocus(node.focus);
    }
}
