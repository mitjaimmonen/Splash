using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



/********************************************
* CanvasOverlayHandler
* 
* Makes some overlay lines and filler image depending on how many players.
* Is Called from playerController with currentPlayerAmount.
*
*/

public class CanvasOverlayHandler : MonoBehaviour {


	public Sprite[] overlaySprites;
	public Image splitter;
	public GameObject filler;

	public void SetOverlay(int players) 
	{
		if (players == 2) 
		{
			splitter.sprite = overlaySprites[0];
			splitter.enabled = true;
			filler.SetActive(false);
		}
		else if (players == 3)
		{
			splitter.sprite = overlaySprites[1];
			splitter.enabled = true;
			filler.SetActive(true);
		}
		else if (players == 4)
		{
			splitter.sprite = overlaySprites[1];
			splitter.enabled = true;
			filler.SetActive(false);
		}
		else 
		{
			splitter.enabled = false;
			filler.SetActive(false);
		}
	}
}
