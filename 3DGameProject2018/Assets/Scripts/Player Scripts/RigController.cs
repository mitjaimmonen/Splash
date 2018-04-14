using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigController : MonoBehaviour {


	private PlayerController playerController;
	private Generics.Dynamics.InverseKinematics inverseKinematics;
	private Animator anim;
	private float vRotation;
	private int layer;
	private bool isAlive = false;



	// Use this for initialization
	void Awake () {
		playerController = GetComponentInParent<PlayerController>();
		inverseKinematics = GetComponent<Generics.Dynamics.InverseKinematics>();
		Debug.Log(inverseKinematics);
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
			rb.sleepThreshold = 0;
		}
		transform.localPosition = Vector3.zero;
		anim.enabled = true;

		
	}

	public void SwitchArmPoints(GameObject left, GameObject right)
	{
		if (left && right && inverseKinematics)
		{
			inverseKinematics.otherChains[0].target = left.transform;
			inverseKinematics.otherChains[1].target = right.transform;		
		}
		else if (inverseKinematics)
		{
			inverseKinematics.otherChains[0].target = null;
			inverseKinematics.otherChains[1].target = null;		
		}
		else
		{
			Debug.Log("No inverseKinematics script found.");
		}

	}

	
	// Update is called once per frame
	void LateUpdate () {
		if (!isAlive && playerController.IsAlive)
		{
			Reset();
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
