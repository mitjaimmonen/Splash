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
        FindController();
        DontDestroyOnLoad(transform.gameObject);
        for(int i = 0; i < players; i++)
        {
            options.EnablePlayer(i);
        }
    }


    /// <summary>
    /// Finds the Controller object in scene
    /// </summary>
    private void FindController()
    {
        controller = GameObject.FindGameObjectWithTag("Controller").GetComponent<IController>();
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
    private void OnLevelWasLoaded(int level)
    {
        controller = GameObject.FindGameObjectWithTag("Controller").GetComponent<IController>();
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
        //listen for input
        for(int i = 0; i < CONTROLLERCOUNT; i++)
        {
            //passes the controller number and button and intensity if applicable ie the triggers
            //if its onpress or onrelease maybe if necessary
            //pass input to current states controller in the scene whatever it may be
            if(Input.GetAxis("Joy" + i + "Start") != 0)
            {
                input[0] = i.ToString();
                input[1] = "Start";
                input[2] = Input.GetAxis("Joy" + i + "Start").ToString();
                controller.InputHandle(input);
            }
            if(Input.GetAxis("Joy" + i +"LeftHorizontal") != 0)
            {
                input[0] = i.ToString();
                input[1] = "LeftHorizontal";
                input[2] = Input.GetAxis("Joy" + i + "LeftHorizontal").ToString();
                controller.InputHandle(input);
            }
            if(Input.GetAxis("Joy" + i + "LeftVertical") != 0)
            {
                input[0] = i.ToString();
                input[1] = "LeftVertical";
                input[2] = Input.GetAxis("Joy" + i + "LeftVertical").ToString();
                controller.InputHandle(input);
            }
            if(Input.GetAxis("Joy" + i + "RightHorizontal") != 0)
            {
                input[0] = i.ToString();
                input[1] = "RightHorizontal";
                input[2] = Input.GetAxis("Joy" + i + "RightHorizontal").ToString();
                controller.InputHandle(input);
            }
            if(Input.GetAxis("Joy" + i + "RightVertical") != 0)
            {
                input[0] = i.ToString();
                input[1] = "RightVertical";
                input[2] = Input.GetAxis("Joy" + i + "RightVertical").ToString();
                controller.InputHandle(input);
            }
           
            if(Input.GetAxis("Joy" + i + "A") != 0)
            {
                // Debug.Log(Input.GetAxis("Joy" + i + "A")+ "    " + i);
                input[0] = i.ToString(); 
                input[1] = "A";
                input[2] = Input.GetAxis("Joy" + i + "A").ToString();
                controller.InputHandle(input);
            }
            if(Input.GetAxis("Joy" + i + "Y") != 0)
            {
                input[0] = i.ToString();
                input[1] = "Y";
                input[2] = Input.GetAxis("Joy" + i + "Y").ToString();
                controller.InputHandle(input);
            }

            if(Input.GetAxis("Joy" + i + "R1") != 0)
            {
                input[0] = i.ToString();
                input[1] = "R1";
                input[2] = Input.GetAxis("Joy" + i + "R1").ToString();
                controller.InputHandle(input);
            }
            if(Input.GetAxis("Joy" + i + "L1") != 0)
            {
                input[0] = i.ToString();
                input[1] = "L1";
                input[2] = Input.GetAxis("Joy" + i + "L1").ToString();
                controller.InputHandle(input);
            }
            if(Input.GetAxis("Joy" + i + "R2") != 0)
            {
                input[0] = i.ToString();
                input[1] = "R2";
                input[2] = Input.GetAxis("Joy" + i + "R2").ToString();
                controller.InputHandle(input);
            }
            if(Input.GetAxis("Joy" + i + "L2") != 0)
            {
                input[0] = i.ToString();
                input[1] = "L2";
                input[2] = Input.GetAxis("Joy" + i + "L2").ToString();
                controller.InputHandle(input);
            }
            if(Input.GetAxis("Joy" + i + "L3") != 0)
            {
                input[0] = i.ToString();
                input[1] = "L3";
                input[2] = Input.GetAxis("Joy" + i + "L3").ToString();
                controller.InputHandle(input);
            }
        }
    }
}