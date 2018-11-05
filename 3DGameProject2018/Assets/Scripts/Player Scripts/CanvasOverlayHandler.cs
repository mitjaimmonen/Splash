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
	public Text GameTimerText;
	public GameObject GameTimerContainer;

	private MatchController matchController;
	private StateHandler stateHandler;

	void Start()
	{
		matchController = GameObject.FindGameObjectWithTag("Controller").GetComponent<MatchController>();
		stateHandler = GameObject.FindGameObjectWithTag("State Handler").GetComponent<StateHandler>();
	}

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

	void Update()
	{
		if (matchController && stateHandler)
		{
			if (matchController.GameCountdown != 0 && stateHandler.options.CurrentActivePlayers > 1)
			{
				GameTimerText.text = matchController.GameCountdown.ToString();
			}
			else
			{
				GameTimerContainer.SetActive(false);
				GameTimerText.text = "";
			}
		}
	}
}
