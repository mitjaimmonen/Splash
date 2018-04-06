using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recoil : MonoBehaviour {

	public bool affectBullets = true;
    public float recoilAngle = 1f, recoilTime = 0.3f, delay = 0.05f;

	public GameObject body, particles;
	private void Awake()
	{
		foreach (Transform trans in GetComponentInChildren<Transform>(true))
		{
			if (trans.GetComponent<ParticleSystem>())
				particles = trans.gameObject;
			else if (trans.gameObject.name == "Body")
				body = trans.gameObject;
		}
		if (!body || !particles)
			Debug.LogWarning("Recoil script is missing references. Gun model: " + body + ", gun particles: " + particles);
	}

	public void StartRecoil()
	{
		StartCoroutine(Recoiling());
	}	

	private IEnumerator Recoiling()
	{
		if (delay >0)
			yield return new WaitForSeconds(delay);

		float timer = Time.time;
		Vector3 localRot = body.transform.localEulerAngles;
		float rotX,newRotX,t,easeInTime,easeOutTime;
		while (timer > Time.time - recoilTime)
		{
			rotX = localRot.x;

			//Eulers fuckup if they are negative, this converts them to positive
			rotX%=360;
            if(rotX >180)
                rotX-= 360;
 
			newRotX = rotX + recoilAngle;
			t = (Time.time - timer)/recoilTime; 			//Lerp timer
			easeOutTime = Mathf.Sin(t * Mathf.PI * 0.5f); 	//Curves the lerp with ease out
			easeInTime = t*t; 								//Exponential curve (ease in)
			rotX = Mathf.Lerp(rotX, newRotX, easeOutTime); 	//Recoil up
			rotX = Mathf.Lerp(newRotX, 0, t); 		//Recoil return
			

			localRot.x = rotX;

			if (affectBullets)
			{
				//Rotate both gameobjects
				body.transform.localEulerAngles = localRot;
				particles.transform.localEulerAngles = localRot;
			}
			else
			{
				//Rotate gun body
				body.transform.localEulerAngles = localRot;
			}
			
			yield return new WaitForEndOfFrame();
		}

		body.transform.localEulerAngles = Vector3.zero;
		particles.transform.localEulerAngles = Vector3.zero;

		yield break;
	}
}
