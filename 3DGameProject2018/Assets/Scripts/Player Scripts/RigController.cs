using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigController : MonoBehaviour {

	public Transform leftArm, rightArm;
	private Vector3 leftRot, rightRot;
	private PlayerController playerController;
	private Animator anim;
	private float vRotation;
	private int layer;
	private bool isAlive;



	// Use this for initialization
	void Awake () {
		playerController = GetComponentInParent<PlayerController>();
		if (leftArm && rightArm)
		{
			leftRot = leftArm.localRotation.eulerAngles;
			rightRot = rightArm.localRotation.eulerAngles;
		}

		anim = GetComponent<Animator>();

		//Cameras have each one culling mask which they wont render
		//Player model must not be rendered by its own camera.
        layer = LayerMask.NameToLayer("Culling" + playerController.playerNumber);	
		foreach (Transform trans in gameObject.GetComponentInChildren<Transform>(true))
		{
			trans.gameObject.layer = layer;	
		}	
		foreach (Rigidbody rb in gameObject.GetComponentsInChildren<Rigidbody>(true)) {
			rb.isKinematic = true;
		}
		transform.localPosition = Vector3.zero;
		anim.enabled = true;

		
	}

	
	// Update is called once per frame
	void LateUpdate () {
		if (!isAlive && playerController.IsAlive)
		{
			Reset();
		}

		if (isAlive && rightArm && leftArm)
		{
			vRotation = playerController.rotationV;
			Vector3 leftArmRot = leftRot;
			leftArmRot.x -= Mathf.Clamp(vRotation,-90, 25);
			leftArm.localEulerAngles = leftArmRot;
			
			Vector3 rightArmRot = rightRot;
			rightArmRot.x -= vRotation;
			rightArm.localEulerAngles = rightArmRot;
		}
	}

	private void Reset()
	{
		//On reset disable player ragdoll and make rigidbodies kinematic again.
		//Camera cannot see own player anymore
		layer = LayerMask.NameToLayer("Culling" + playerController.playerNumber);
		foreach (Transform trans in gameObject.GetComponentInChildren<Transform>(true))
		{
			trans.gameObject.layer = layer;
		}
		foreach (Rigidbody rb in gameObject.GetComponentsInChildren<Rigidbody>(true))
		{
			rb.isKinematic = true;
		}
		anim.enabled = true;		
		anim.Rebind();
		transform.localPosition = Vector3.zero;
		isAlive = true;
	}

	public void Die()
	{
		//On death player model will become ragdoll and all rigidbodies need to enable physics.
		//Now camera is allowed to render own player.
		isAlive = false;
		anim.enabled = false;
		layer = LayerMask.NameToLayer("Player");
		foreach (Transform trans in gameObject.GetComponentInChildren<Transform>(true))
		{
			trans.gameObject.layer = layer;
		}
		foreach (Rigidbody rb in gameObject.GetComponentsInChildren<Rigidbody>(true))
		{
			rb.isKinematic = false;
		}
	}
}
