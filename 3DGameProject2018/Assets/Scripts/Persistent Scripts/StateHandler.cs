using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using XInputDotNetPure;

/*Enum*/
public enum State
{
    //Enum for the states of the game
    MainMenu, Game, EndMenu
}

/*Interface*/
public interface IController
{
    //Interface for any controller to force implementing input handler
    void InputHandle(string[] input);
}

/*************************************
 * StateHandler class
 *  Monitors all input
 *  Holds the options
 *  Handles scene Changing
 */
public class StateHandler : MonoBehaviour {

    /******************/
    /*Member Variables*/
    public MatchOptions options = new MatchOptions();
    [SerializeField, Tooltip("Default initialized players when starting in game")]
    private int players = 0;
    [HideInInspector]
    public IController controller;
    public State state;
    private GamePadState[] gamepads = new GamePadState[4];
    public List<PlayerStats> stats = new List<PlayerStats>();



    /************************/
    /*MonoBehavior Functions*/
    //Makes sure no duplicate state handler, and when not started in main menu initializes some players
    private void Awake()
    {
        //Make sure there isnt an active state Handler(basicly only tripped when reloading the main menu)
        if(GameObject.FindGameObjectsWithTag("State Handler").Length > 1)
        {
            Destroy(gameObject);
        }
        if(state != State.MainMenu)
        {
            for(int i = 0; i < players; i++)
            {
                options.EnablePlayer(i);
            }
        }
        FindController();
        DontDestroyOnLoad(transform.gameObject);
    }
    //Looks for input from tracked controllers every update
    void Update()
    {
        FindInputXInput();
    }
    //When loading new scene find a controller
    private void OnLevelWasLoaded(int level)
    {
        controller = null;
        FindController();
    }



    #region Public Functions
    
    /// <summary>
    /// Handles changing the game between scenes
    /// </summary>
    public void ChangeState(State desiredState)
    {
        switch(desiredState)
        {
            case State.MainMenu:
                SceneManager.LoadScene("MainMenu");
                break;
            case State.Game:
                // if we have more than one map we will look at options map and call the associated scene
                SceneManager.LoadScene("Level1");
                break;
            case State.EndMenu:
                SceneManager.LoadScene("EndScene");
                break;
            default:
                break;
        }
    }
    /// <summary>
    /// Sorts the current list of stats objects
    /// </summary>
    public void Sort()
    {
        List<PlayerStats> temp = new List<PlayerStats>();
        int length = stats.Count;
        for(int i = 0; i < length; i++)
        {
            int highest = FindHighest(stats);
            temp.Add(stats[highest]);
            stats.RemoveAt(highest);
        }
        for(int i = 0; i < temp.Count; i++)
        {
            stats.Add(temp[i]);
        }
    }

    #endregion



    #region Private Functions
    
    /// <summary>
    /// Finds the Controller object in scene
    /// </summary>
    private void FindController()
    {
        IController temp = GameObject.FindGameObjectWithTag("Controller").GetComponent<IController>();
        if(temp != null)
        {
            controller = GameObject.FindGameObjectWithTag("Controller").GetComponent<IController>();
        } else
        {
            Debug.Log("No Controller found on scene load");
        }
    }
    /// <summary>
    /// Return the index of the List<PlayerStats> element with most kills
    /// </summary>
    /// <param name="temp">List<PlayerStats> to search</param>
    /// <returns>Element Index</returns>
    private int FindHighest(List<PlayerStats> temp) {
        int index = 0;
        int kills = 0;
        for(int i = 0; i < temp.Count; i++)
        {
            if(temp[i].kills > kills)
            {
                kills = temp[i].kills;
                index = i;
            }
        }
        return index;
    }
    /// <summary>
    /// Finds Xinput input of active controllers
    /// </summary>
    private void FindInputXInput()
    {
        if(controller == null)
        {
            return;
        }
        string[] input = new string[3];//String[] to hold data of event to be passed
        int skippedplayer = 0;//Tracks if we skip a player, in case of gaps in active players
        for(int i = 0; i < options.CurrentActivePlayers; i++)
        {
            //If player is inactive skip this loop and add to skippedplayer
            if(options.PlayersInfo[i, 2] == 0)
            {
                skippedplayer++;
                continue;
            } else
            {//else update the current gamepads object
                //if this isnt done the current state of the controller wont be updated, so never remove this
                gamepads[i] = GamePad.GetState((PlayerIndex)options.PlayersInfo[i, 3]);
            }
            //Look for pressed buttons and send the info to our current IController
            /* IMPORTANT : Not all buttons are being listened to right now, but you can just add ones if needed*/
            if(gamepads[i].Buttons.Start == ButtonState.Pressed)
            {
                input[0] = (i - skippedplayer).ToString();
                input[1] = "Start";
                input[2] = "1";
                controller.InputHandle(input);
            }
            if(gamepads[i].ThumbSticks.Left.X != 0)
            {
                input[0] = (i - skippedplayer).ToString();
                input[1] = "LeftHorizontal";
                input[2] = (gamepads[i].ThumbSticks.Left.X).ToString();
                controller.InputHandle(input);
            }
            if(gamepads[i].ThumbSticks.Left.Y != 0)
            {
                input[0] = (i - skippedplayer).ToString();
                input[1] = "LeftVertical";
                input[2] = (-gamepads[i].ThumbSticks.Left.Y).ToString();
                controller.InputHandle(input);
            }
            if(gamepads[i].ThumbSticks.Right.X != 0)
            {
                input[0] = (i - skippedplayer).ToString();
                input[1] = "RightHorizontal";
                input[2] = (gamepads[i].ThumbSticks.Right.X).ToString();
                controller.InputHandle(input);
            }
            if(gamepads[i].ThumbSticks.Right.Y != 0)
            {
                input[0] = (i - skippedplayer).ToString();
                input[1] = "RightVertical";
                input[2] = (-gamepads[i].ThumbSticks.Right.Y).ToString();
                controller.InputHandle(input);
            }
            if(gamepads[i].Buttons.A == ButtonState.Pressed)
            {
                input[0] = (i - skippedplayer).ToString();
                input[1] = "A";
                input[2] = "1";
                controller.InputHandle(input);
            }
            if(gamepads[i].Buttons.B == ButtonState.Pressed)
            {
                input[0] = (i - skippedplayer).ToString();
                input[1] = "B";
                input[2] = "1";
                controller.InputHandle(input);
            }
            if(gamepads[i].Buttons.Y == ButtonState.Pressed)
            {
                input[0] = (i - skippedplayer).ToString();
                input[1] = "Y";
                input[2] = "1";
                controller.InputHandle(input);
            }

            if(gamepads[i].Buttons.RightShoulder == ButtonState.Pressed)
            {
                input[0] = (i - skippedplayer).ToString();
                input[1] = "R1";
                input[2] = "1";
                controller.InputHandle(input);
            }
            if(gamepads[i].Buttons.LeftShoulder == ButtonState.Pressed)
            {
                input[0] = (i - skippedplayer).ToString();
                input[1] = "L1";
                input[2] = "1";
                controller.InputHandle(input);
            }
            if(gamepads[i].Triggers.Right != 0)
            {
                input[0] = (i - skippedplayer).ToString();
                input[1] = "R2";
                input[2] = gamepads[i].Triggers.Right.ToString();
                controller.InputHandle(input);
            }
            if(gamepads[i].Triggers.Left != 0)
            {
                input[0] = (i - skippedplayer).ToString();
                input[1] = "L2";
                input[2] = gamepads[i].Triggers.Left.ToString();
                controller.InputHandle(input);
            }
            if(gamepads[i].Buttons.LeftStick == ButtonState.Pressed)
            {
                input[0] = (i - skippedplayer).ToString();
                input[1] = "L3";
                input[2] = "1";
                controller.InputHandle(input);
            }
        }
    }

    #endregion



    /*******************/
    /*Legacy Functions*/
    /// <summary>
    /// Finds Unity inputs of active controllers
    /// </summary>
    private void FindInputUnity()
    {
        
        string[] input = new string[3];
        int currentController;
        int skippedplayer = 0;
        //listen for input
        for(int i = 0; i < options.CurrentActivePlayers; i++)
        {
            if(options.PlayersInfo[i,2] ==0)
            {
                skippedplayer++;
                continue;
            }
            currentController = options.PlayersInfo[i, 3];
            //passes the controller number and button and intensity if applicable ie the triggers
            //if its onpress or onrelease maybe if necessary
            //pass input to current states controller in the scene whatever it may be
            if(Input.GetAxis("Joy" + currentController + "Start") != 0)
            {
                input[0] = (i-skippedplayer).ToString();
                input[1] = "Start";
                input[2] = Input.GetAxis("Joy" + currentController + "Start").ToString();
                controller.InputHandle(input);
            }
            if(Input.GetAxis("Joy" + currentController + "LeftHorizontal") != 0)
            {
                input[0] = (i - skippedplayer).ToString();
                input[1] = "LeftHorizontal";
                input[2] = Input.GetAxis("Joy" + currentController + "LeftHorizontal").ToString();
                controller.InputHandle(input);
            }
            if(Input.GetAxis("Joy" + currentController + "LeftVertical") != 0)
            {
                input[0] = (i - skippedplayer).ToString();
                input[1] = "LeftVertical";
                input[2] = Input.GetAxis("Joy" + currentController + "LeftVertical").ToString();
                controller.InputHandle(input);
            }
            if(Input.GetAxis("Joy" + currentController + "RightHorizontal") != 0)
            {
                input[0] = (i - skippedplayer).ToString();
                input[1] = "RightHorizontal";
                input[2] = Input.GetAxis("Joy" + currentController + "RightHorizontal").ToString();
                controller.InputHandle(input);
            }
            if(Input.GetAxis("Joy" + currentController + "RightVertical") != 0)
            {
                input[0] = (i - skippedplayer).ToString();
                input[1] = "RightVertical";
                input[2] = Input.GetAxis("Joy" + currentController + "RightVertical").ToString();
                controller.InputHandle(input);
            }
           
            if(Input.GetAxis("Joy" + currentController + "A") != 0)
            {
                input[0] = (i - skippedplayer).ToString(); 
                input[1] = "A";
                input[2] = Input.GetAxis("Joy" + currentController + "A").ToString();
                controller.InputHandle(input);
            }
            if(Input.GetAxis("Joy" + currentController + "Y") != 0)
            {
                input[0] = (i - skippedplayer).ToString();
                input[1] = "Y";
                input[2] = Input.GetAxis("Joy" + currentController + "Y").ToString();
                controller.InputHandle(input);
            }

            if(Input.GetAxis("Joy" + currentController + "R1") != 0)
            {
                input[0] = (i - skippedplayer).ToString();
                input[1] = "R1";
                input[2] = Input.GetAxis("Joy" + currentController + "R1").ToString();
                controller.InputHandle(input);
            }
            if(Input.GetAxis("Joy" + currentController + "L1") != 0)
            {
                input[0] = (i - skippedplayer).ToString();
                input[1] = "L1";
                input[2] = Input.GetAxis("Joy" + currentController + "L1").ToString();
                controller.InputHandle(input);
            }
            if(Input.GetAxis("Joy" + currentController + "R2") != 0)
            {
                input[0] = (i - skippedplayer).ToString();
                input[1] = "R2";
                input[2] = Input.GetAxis("Joy" + currentController + "R2").ToString();
                controller.InputHandle(input);
            }
            if(Input.GetAxis("Joy" + currentController + "L2") != 0)
            {
                input[0] = (i - skippedplayer).ToString();
                input[1] = "L2";
                input[2] = Input.GetAxis("Joy" + currentController + "L2").ToString();
                controller.InputHandle(input);
            }
            if(Input.GetAxis("Joy" + currentController + "L3") != 0)
            {
                input[0] = (i - skippedplayer).ToString();
                input[1] = "L3";
                input[2] = Input.GetAxis("Joy" + currentController + "L3").ToString();
                controller.InputHandle(input);
            }
        }
    }
   

}