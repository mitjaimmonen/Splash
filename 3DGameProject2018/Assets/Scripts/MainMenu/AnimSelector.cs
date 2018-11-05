using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimSelector : MonoBehaviour {
    public string anim;
	// Use this for initialization
	void Start () {

        GetComponent<Animator>().Play(anim);
    }
    public void Activate() {
        
        gameObject.SetActive(true);
        GetComponent<Animator>().Play(anim);
    }
}
