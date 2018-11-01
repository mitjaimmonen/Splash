using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMover : MonoBehaviour {

    /******************/
    /*Member Variables*/
    private Vector3 startPos;
    private Quaternion startRot;
    public MovementNode[] transformNodes;

    private Vector3 lerpStartPos;
    private Quaternion lerpStartRot;
    private int targetNode;
    private float totalTime;
    public bool isMoving = false;
    /************************/
    /*MonoBehavior Functions*/
    //shows the currently active players and starts music
    private void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;
        for(int i = 0; i < transformNodes.Length; i++)
        {
            transformNodes[i].nodeNumber = i;
        }
    }

    public MovementNode ToNode(int node, float time) {
        if(!isMoving)
        {
            isMoving = true;
            lerpStartPos = transform.position;
            lerpStartRot = transform.rotation;
            targetNode = node;
            totalTime = time;
            StartCoroutine("Move");
        }
        return transformNodes[targetNode];
    }
    IEnumerator Move()
    {
        float elapsedTime = 0;
        while(transform.position != transformNodes[targetNode].transform.position )
        {
            elapsedTime += Time.deltaTime;
            transform.position = Vector3.Lerp(lerpStartPos, transformNodes[targetNode].transform.position, elapsedTime/totalTime);
            transform.rotation = Quaternion.Slerp(lerpStartRot, transformNodes[targetNode].transform.rotation, elapsedTime / totalTime);
            yield return null;
        }

        isMoving = false;
    }
}
