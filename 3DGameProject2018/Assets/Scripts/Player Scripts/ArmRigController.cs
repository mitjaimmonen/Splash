using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmRigController : MonoBehaviour {

	private PlayerController playerController;
	private Generics.Dynamics.InverseKinematics inverseKinematics;
	private int layer;
	private bool isAlive = false;

	void Awake()
	{
		playerController = GetComponentInParent<PlayerController>();
		inverseKinematics = GetComponent<Generics.Dynamics.InverseKinematics>();

		//Cameras have each one culling mask which they wont render
		//Player model must not be rendered by its own camera.
        layer = LayerMask.NameToLayer("Culling" + playerController.playerNumber);	
		foreach (Transform trans in gameObject.GetComponentInChildren<Transform>(true))
		{
			trans.gameObject.layer = layer;	
		}
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
			Debug.Log("No Arms found, setting inverseKinematics values to null");
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
		isAlive = true;
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
	}

	public void Die()
	{
		isAlive = false;
		//On death player model will become ragdoll and all rigidbodies need to enable physics.
		//Now camera is allowed to render own player.
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
