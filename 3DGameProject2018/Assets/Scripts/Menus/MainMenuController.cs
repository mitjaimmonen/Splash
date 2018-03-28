using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XInputDotNetPure;

/********************************************
 * MainMenuController class
 *  Handles activating players
 *  Sets the game options
 *  Starts the match
 */
public class MainMenuController : MonoBehaviour, IController {

    /******************/
    /*Member Variables*/
    private MatchOptions options = new MatchOptions();
    public TextMeshProUGUI[] visualPlayerElements = new TextMeshProUGUI[4];
    private StateHandler stateHandler;



    /************************/
    /*MonoBehavior Functions*/
    //shows the currently active players and starts music
    private void Start()
    {
        stateHandler = GameObject.FindGameObjectWithTag("State Handler").GetComponent<StateHandler>();
        for(int i = 0; i < stateHandler.options.PlayersInfo.GetLength(0); i++)
        {
            if(stateHandler.options.PlayersInfo[i,2]==1)
            {
                visualPlayerElements[i].text = "Player " + (i+1) + " Ready";
            }
        }

    }
    //Listen for controllers to add them to current players
    private void Update()
    {
        for(int i = 0; i < 6; i++)
        {
            if(GamePad.GetState((PlayerIndex)i).Buttons.Start == ButtonState.Pressed)//if(Input.GetAxis("Joy" + i + "Start") != 0)
            {
                AddPlayer(i);
            }
        }
    }



    /*****************/
    /*Implementations*/
    //Icontroller implementation, listens to exit a player
    public void InputHandle(string[] input)
    {
        if(input[1] == "Y")
        {
            RemovePlayer(stateHandler.options.PlayersInfo[int.Parse(input[0]), 3]);
        }
    }



    #region Public Functions

    /// <summary>
    /// Call state change to game
    /// Only works if there are active players
    /// </summary>
    public void StartGame()
    {
        //If there's an active player pass options to StateHandler and change state
        if(stateHandler.options.CurrentActivePlayers >0)
        {
            stateHandler.ChangeState(State.Game);
        }
    }
    /// <summary>
    /// Change the max game time in options
    /// </summary>
    /// <param name="time">Slider that the value is pulled from</param>
    public void ChangeTime(Slider time)
    {
        stateHandler.options.maxTime = time.value;
    }
    /// <summary>
    /// Change the max kills in options
    /// </summary>
    /// <param name="time">Slider that the value is pulled from</param>
    public void ChangeKills(Slider kills)
    {
        stateHandler.options.maxKills = kills.value;
    }
    /// <summary>
    /// Quits Game to desktop
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }
    
    #endregion



    #region Private Functions

    /// <summary>
    /// Binds controller to an activated player
    /// </summary>
    /// <param name="controller">Xinput number of controller</param>
    private void AddPlayer(int controller)
    {
        if(stateHandler.options.EnablePlayer(controller))
        {
            int currentplayer = stateHandler.options.PlayerFromController(controller);
            visualPlayerElements[currentplayer].text = "Player " + (currentplayer+1) + " Ready";
        }
    }
    /// <summary>
    /// Deactivates player
    /// </summary>
    /// <param name="controller">Xinput number of controller</param>
    private void RemovePlayer(int controller)
    {
        int currentplayer = stateHandler.options.PlayerFromController(controller);
        if(stateHandler.options.DisablePlayer(controller))
        {
            visualPlayerElements[currentplayer].text = "Press Start";
        }
    }
    
    #endregion


    /******************************
     *            To Do
     *  Add settings prefab and
     *  ShowSettings function
     *  
    //Brings up settings menu prefab
    //changes the states current icontroller to the new settings
    //makes settings last controller to this object
    private void ShowSettings(){}
     ******************************/
    
}
