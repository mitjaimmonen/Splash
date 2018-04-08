using UnityEngine;
using System.Collections;

public class FlockObject : MonoBehaviour
{
    private GameObject Controller;
    private bool inited = false;
    private float minVelocity;
    private float maxVelocity;
    private float randomness;
    private GameObject chasee;
    private Rigidbody rigid;
    void Start()
    {
        rigid = GetComponent<Rigidbody>();
        StartCoroutine("BoidSteering");
    }
    

    IEnumerator BoidSteering()
    {
        while(true)
        {
            if(inited)
            {
                rigid.velocity = rigid.velocity + Calc() * Time.deltaTime;

                // enforce minimum and maximum speeds for the boids
                float speed = rigid.velocity.magnitude;
                if(speed > maxVelocity)
                {
                    rigid.velocity = rigid.velocity.normalized * maxVelocity;
                } else if(speed < minVelocity)
                {
                    rigid.velocity = rigid.velocity.normalized * minVelocity;
                }
                transform.rotation = Quaternion.LookRotation(rigid.velocity, Vector3.up);
            }

            float waitTime = Random.Range(0.2f, 0.3f);
            yield return new WaitForSeconds(waitTime);
        }
    }

    private Vector3 Calc()
    {
        Vector3 randomize = new Vector3((Random.value * 2) - 1, (Random.value * 2) - 1, (Random.value * 2) - 1);

        randomize.Normalize();
        FlockController flockController = Controller.GetComponent<FlockController>();
        Vector3 flockCenter = flockController.flockCenter;
        Vector3 flockVelocity = flockController.flockVelocity;
        Vector3 follow = chasee.transform.localPosition;

        flockCenter = flockCenter - transform.localPosition;
        flockVelocity = flockVelocity - GetComponent<Rigidbody>().velocity;
        follow = follow - transform.localPosition;

        return (flockCenter + flockVelocity + follow * 2 + randomize * randomness);
    }

    public void SetController(GameObject theController)
    {
        Controller = theController;
        FlockController flockController = Controller.GetComponent<FlockController>();
        minVelocity = flockController.minVelocity;
        maxVelocity = flockController.maxVelocity;
        randomness = flockController.randomness;
        chasee = flockController.chasee;
        inited = true;
    }
}