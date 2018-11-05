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


	private float rotX, newRotX, rotTime, rotReturnTime, rotBeginTime;
	private float posZ, newPosZ, posTime, posReturnTime, posBeginTime;
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

		float timer = Time.time, rotEndTime = 0;
		Vector3 localRot = body.transform.localEulerAngles;
		float startRotX = localRot.x;
		newRotX = startRotX + recoilAngle;

		while (timer > Time.time - rotationTime)
		{
			// rotX = localRot.x;

			rotTime = (Time.time - timer)/rotationTime; //Lerp timer
			rotEndTime = (rotTime - recoilStartTimeDivider) / (1-recoilStartTimeDivider);

			rotBeginTime = Mathf.Sin((rotTime/recoilStartTimeDivider) * Mathf.PI * 0.5f); //Ease out
			rotReturnTime = rotEndTime*rotEndTime * (3f - 2f*rotEndTime); //SmoothStep

			//Eulers fuckup if they are negative, this converts them to positive
			startRotX%=360;  
			if(startRotX >180)
				startRotX-= 360;
			
			newRotX%=360;
			if (newRotX > 180)
				newRotX-=360;
			if (newRotX < -85)
				newRotX = -85f;
			
			if (rotTime < recoilStartTimeDivider)
			{
				//Recoil up
				rotX = Mathf.Lerp(startRotX, newRotX, rotBeginTime);
			}
			else if (rotTime <= 1f)
			{
				//Recoil return
				rotX = Mathf.Lerp(newRotX, oldRot.x, rotReturnTime);
			}

			rotX = Mathf.Lerp(rotX, oldRot.x, rotTime);

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

		float timer = Time.time, posEndTime=0;
		Vector3 localPosBody = body.transform.localPosition;
		float startPosZ = localPosBody.z;
		newPosZ = startPosZ + recoilPosition;


		while (timer > Time.time - positionTime)
		{
			posTime = (Time.time - timer)/positionTime;
			posEndTime = (posTime - recoilStartTimeDivider) / (1-recoilStartTimeDivider);

			posBeginTime = Mathf.Sin((posTime / recoilStartTimeDivider) * Mathf.PI * 0.5f); //Ease out
			posReturnTime = posEndTime * posEndTime * (3f - 2f*posEndTime); //SmoothStep

			if (posTime < recoilStartTimeDivider)
				posZ = Mathf.Lerp(startPosZ, newPosZ, posBeginTime);
			else
				posZ = Mathf.Lerp(newPosZ, oldPosBody.z, posReturnTime);

			localPosBody.z = posZ;
			body.transform.localPosition = localPosBody;

			yield return null;

		}
		body.transform.localPosition = oldPosBody;
		yield break;

	}
}
