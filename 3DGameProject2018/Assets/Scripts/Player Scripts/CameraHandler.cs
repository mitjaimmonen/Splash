using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHandler : MonoBehaviour {

    public GameObject target;
    private void Update()
    {
        transform.position = target.transform.position;
        // var rot = transform.rotation;
        // rot.y = player.transform.rotation.y;
        transform.rotation = target.transform.rotation;
    }

}
