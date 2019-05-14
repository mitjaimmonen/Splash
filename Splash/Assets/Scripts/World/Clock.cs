using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clock : MonoBehaviour
{
    public GameObject hourHand;
    public GameObject minuteHand;
    public Vector3 rotationAxis;
    
    // Update is called once per frame
    void Update()
    {
        System.DateTime currentTime = System.DateTime.Now;
        float second = currentTime.Second;
        float minute = currentTime.Minute + second / 60f;
        float hour = (currentTime.Hour % 12) + minute / 60f;
        minuteHand.transform.localRotation = Quaternion.Euler(0, 0, -360 * (minute / 60f));
        hourHand.transform.localRotation = Quaternion.Euler(0,0, -360 * (hour / 12f));
        
    }
}
