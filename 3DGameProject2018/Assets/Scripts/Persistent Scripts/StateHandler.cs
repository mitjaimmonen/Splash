using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
/********************************************
 * StateHandler class
 *  Monitors all input
 *  Holds the options
 *  Handles scene Changing
 */
public enum State
{
    MainMenu, Game, EndMenu
}
interface IController
{
    void InputHandle(string[] input);
}
public class StateHandler : MonoBehaviour
{

    public MatchOptions options = new MatchOptions();//Current Options
    [SerializeField, Tooltip("Default initialized players")]
    public int players = 0;
    private IController controller;//Controller for current Scene
    public State state;//Current State
    private const int CONTROLLERCOUNT = 4;

    
    
    //Makes sure no duplicate state handler, and temporary launches straight to map
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

    private void OnLevelWasLoaded(int level)
    {
        FindController();
    }


    /// <summary>
    /// Finds the Controller object in scene
    /// </summary>
    private void FindController()
    {
        var temp = GameObject.FindGameObjectWithTag("Controller").GetComponent<IController>();
        if(temp != null)
        {
            controller = GameObject.FindGameObjectWithTag("Controller").GetComponent<IController>();
        } else
        {
            Debug.Log("No Controller found on scene load");
        }
    }



    /// <summary>
    /// Handles changing the game between scenes
    /// </summary>
    public void ChangeState(State desiredState)
    {
        //if main start main
        //if end start end
        //if game load scene of map
        //pass match controller the options and teams
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
                break;
            default:
                break;
        }
    }
    



    void Update()
    {
        FindInput();
    }



    /// <summary>
    /// Listens for all inputs and passes to current controller
    /// </summary>
    private void FindInput()
    {
        string[] input = new string[3];
        int currentController;
        int skippedplayer = 0;
        //listen for input
        for(int i = 0; i < CONTROLLERCOUNT; i++)
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