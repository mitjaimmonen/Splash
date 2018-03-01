using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasOverlayHandler : MonoBehaviour {

	// Use this for initialization
	public Sprite[] overlaySprites;
	public Image splitter, filler;


	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SetOverlay(int players) 
	{
		if (players == 2) 
		{
			splitter.sprite = overlaySprites[0];
			splitter.enabled = true;
			filler.enabled = false;
		}
		else if (players == 3)
		{
			splitter.sprite = overlaySprites[1];
			splitter.enabled = true;
			filler.enabled = true;
		}
		else if (players == 4)
		{
			splitter.sprite = overlaySprites[1];
			splitter.enabled = true;
			filler.enabled = false;
		}
		else 
		{
			splitter.enabled = false;
			filler.enabled = false;
		}
	}
}
