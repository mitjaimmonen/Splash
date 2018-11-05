using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InAnimationCalls : MonoBehaviour {

/*******************************
*InAnimationCalls
*
* This class can be added to any object with animator which needs functions called from animations.
* 
* Note: Only weapon is referenced at the moment.
*
*
 */
	private Weapon weapon;

	private void Awake() {
		weapon = GetComponentInParent<Weapon>();
	}

	public void ReloadStart() {
		weapon.AnimReloadStarted();
	}
	public void ReloadEnd() {
		weapon.AnimReloadEnded();
	}

}
