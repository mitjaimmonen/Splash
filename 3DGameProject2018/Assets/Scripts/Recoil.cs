using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recoil : MonoBehaviour {

	public bool affectBullets = true;
    public float rotationTime = 0.3f, positionTime = 0.1f;
	public float rotDelay = 0.05f, posDelay = 0.05f;
	public float recoilAngle = 1f, recoilPosition = 0.5f;
	private GameObject body, particles;


	private float rotX, newRotX, rotTime, rotEaseInTime, rotEaseOutTime;
	private float posZ, newPosZ, posTime, posEaseInTime, posEaseOutTime;
	private Vector3 oldPosBody, oldPosParticles, oldRot;


	public GameObject WeaponBody
	{
		get { return body.gameObject; }
	}

	private void Awake()
	{
		foreach (Transform trans in GetComponentInChildren<Transform>(true))
		{
			var temp = trans.GetComponentInChildren<ParticleSystem>();
			if (temp)
				particles = temp.gameObject;
			else if (trans.gameObject.name == "Body")
				body = trans.gameObject;
		}
		if (!body || !particles)
			Debug.LogWarning("Recoil script is missing references. Gun model: " + body + ", gun particles: " + particles);

		oldRot = body.transform.localEulerAngles;
		oldPosBody = body.transform.localPosition;
		oldPosParticles = particles.transform.localPosition;
		 
	}

	public void StartRecoil()
	{
		StartCoroutine(PlayKnockback());
		StartCoroutine(PlayRecoil());
	}	

	private IEnumerator PlayRecoil()
	{
		if (rotDelay >0)
			yield return new WaitForSeconds(rotDelay);

		float timer = Time.time;
		Vector3 localRot = body.transform.localEulerAngles;

		while (timer > Time.time - rotationTime)
		{
			rotX = localRot.x;

			rotTime = (Time.time - timer)/rotationTime; 			//Lerp timer
			rotEaseOutTime = Mathf.Sin(rotTime * Mathf.PI * 0.5f); 	//Curves the lerp with ease out
			rotEaseInTime = rotTime*rotTime; 						//Exponential curve (ease in)

			//Eulers fuckup if they are negative, this converts them to positive
			rotX%=360;  
			if(rotX >180)
				rotX-= 360;

			newRotX = rotX + recoilAngle;
			rotX = Mathf.Lerp(rotX, newRotX, rotEaseOutTime); 		//Recoil rotation start
			rotX = Mathf.Lerp(newRotX, oldRot.x, rotEaseInTime); 	//Recoil rotation return
			localRot.x = rotX;



			body.transform.localEulerAngles = localRot;

			if (affectBullets)
				particles.transform.localEulerAngles = localRot;
			
			
			yield return null;
		}

		//Set everything back just in case.
		body.transform.localEulerAngles = oldRot;
		particles.transform.localEulerAngles = oldRot;
		yield break;
	}

	private IEnumerator PlayKnockback()
	{
		if (posDelay >0)
			yield return new WaitForSeconds(posDelay);

		float timer = Time.time;
		Vector3 localPosBody = body.transform.localPosition;
		Vector3 localPosParticles = particles.transform.localPosition;


		while (timer > Time.time - positionTime)
		{
			posTime = (Time.time - timer)/positionTime; 			//Lerp timer
			posEaseOutTime = Mathf.Sin(posTime * Mathf.PI * 0.5f); 	//Curves the lerp with ease out
			posEaseInTime = posTime*posTime; 						//Exponential curve (ease in)

			newPosZ = posZ + recoilPosition;
			posZ = Mathf.Lerp(posZ, newPosZ, posEaseOutTime);
			posZ = Mathf.Lerp(newPosZ, oldPosBody.z, posEaseInTime);
			localPosBody.z = posZ;
			// localPosParticles.z = posZ;

			body.transform.localPosition = localPosBody;

			// if (affectBullets)
				// particles.transform.localPosition = localPosParticles;
			
			yield return null;

		}
		body.transform.localPosition = oldPosBody;
		// particles.transform.localPosition = oldPosParticles;
		yield break;

	}
}
