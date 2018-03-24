using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigController : MonoBehaviour {

	public Transform leftArm, rightArm;
	private Vector3 leftRot, rightRot;
	private PlayerController playerController;
	private float vRotation;
	int layer;
	// Use this for initialization
	void Awake () {
		playerController = GetComponentInParent<PlayerController>();
		leftRot = leftArm.localRotation.eulerAngles;
		rightRot = rightArm.localRotation.eulerAngles;

        layer = LayerMask.NameToLayer("Culling" + playerController.playerNumber);		
		foreach (Transform trans in gameObject.GetComponentsInChildren<Transform>(true)) {
			trans.gameObject.layer = layer;
		}
		
	}
	
	// Update is called once per frame
	void LateUpdate () {
		vRotation = playerController.rotationV;
		Vector3 leftArmRot = leftRot;
		leftArmRot.x -= Mathf.Clamp(vRotation,-90, 25);
		leftArm.localEulerAngles = leftArmRot;
		
		Vector3 rightArmRot = rightRot;
		rightArmRot.x -= vRotation;
		rightArm.localEulerAngles = rightArmRot;
		
	}
}
