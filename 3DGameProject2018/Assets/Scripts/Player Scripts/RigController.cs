using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigController : MonoBehaviour {

	public Transform leftArm, rightArm;

	private Vector3 leftRot, rightRot;
	private PlayerController playerController;
	private float vRotation;
	// Use this for initialization
	void Start () {
		playerController = GetComponentInParent<PlayerController>();
		leftRot = leftArm.localRotation.eulerAngles;
		rightRot = rightArm.localRotation.eulerAngles;
	}
	
	// Update is called once per frame
	void LateUpdate () {
		vRotation = playerController.rotationV;
		Vector3 leftArmRot = leftRot;
		leftArmRot.x -= vRotation;
		leftArm.localEulerAngles = leftArmRot;
		
		Vector3 rightArmRot = rightRot;
		rightArmRot.x -= vRotation;
		rightArm.localEulerAngles = rightArmRot;
		Debug.Log(vRotation + ", " + rightArm.localEulerAngles);
		
	}
}
