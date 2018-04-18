using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recoil : MonoBehaviour {

	[Range(0f,1f)]
	public float recoilStartTimeDivider = 0.5f;

	// public bool affectBullets = true;
    public float rotationTime = 0.3f, positionTime = 0.1f;
	public float rotDelay = 0.05f, posDelay = 0.05f;
	public float recoilAngle = 1f, recoilPosition = 0.5f;
	private GameObject body;


	private float rotX, newRotX, rotTime, rotEndTime, rotStartTime;
	private float posZ, newPosZ, posTime, posEaseInTime, posEaseOutTime;
	private Vector3 oldPosBody, oldRot;


	public GameObject WeaponBody
	{
		get { return body.gameObject; }
	}

	private void Awake()
	{
		foreach (Transform trans in GetComponentInChildren<Transform>(true))
		{
			if (trans.gameObject.name == "Body")
				body = trans.gameObject;
		}
		if (!body)
			Debug.LogWarning("Recoil script is missing references. Gun model: " + body);

		oldRot = body.transform.localEulerAngles;
		oldPosBody = body.transform.localPosition;		 
	}

	void OnDisable()
	{
		//Set everything back just in case.
		body.transform.localEulerAngles = oldRot;
	}

	public void StartRecoil()
	{
		if (recoilPosition != 0)
			StartCoroutine(PlayKnockback());
		if (recoilAngle != 0)
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

			rotTime = (Time.time - timer)/rotationTime; //Lerp timer
			rotStartTime = Mathf.Sin(rotTime * Mathf.PI * 0.5f) / recoilStartTimeDivider; //Ease out
			rotEndTime = rotTime*rotTime; //Ease in

			//Eulers fuckup if they are negative, this converts them to positive
			rotX%=360;  
			if(rotX >180)
				rotX-= 360;

			newRotX = rotX + recoilAngle;
			if (rotStartTime <= 1)
				rotX = Mathf.Lerp(rotX, newRotX, rotStartTime); //Recoil rotation start
			rotX = Mathf.Lerp(newRotX, oldRot.x, rotTime); 		//Recoil rotation return
			localRot.x = rotX;

			body.transform.localEulerAngles = localRot;			
			
			yield return null;
		}

		//Set everything back just in case.
		body.transform.localEulerAngles = oldRot;
		yield break;
	}

	private IEnumerator PlayKnockback()
	{
		if (posDelay >0)
			yield return new WaitForSeconds(posDelay);

		float timer = Time.time;
		Vector3 localPosBody = body.transform.localPosition;

		while (timer > Time.time - positionTime)
		{
			posTime = (Time.time - timer)/positionTime;
			posEaseOutTime = Mathf.Sin(posTime * Mathf.PI * 0.5f) / recoilStartTimeDivider;
			posEaseInTime = posTime*posTime;

			newPosZ = oldPosBody.z + recoilPosition;
			if (posEaseOutTime <= 1)
				posZ = Mathf.Lerp(posZ, newPosZ, posEaseOutTime);
				
			posZ = Mathf.Lerp(posZ, oldPosBody.z, posEaseInTime);
			localPosBody.z = posZ;
			body.transform.localPosition = localPosBody;

			yield return null;

		}
		body.transform.localPosition = oldPosBody;
		yield break;

	}
}
