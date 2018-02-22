using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    private MatchOptions options;
    //we will search and find this during change state
    public GameObject controllerObject;
    private IController controller;//this is gonna be empty in all but the initial menu
    public State state;

    //handle changing the state
    public void ChangeState(State desiredState)
    {
        //if main start main
        //if end start end
        //if game load scene of map
        //pass match controller the options and teams
    }
    private void Start()
    {
        controller = controllerObject.GetComponent<IController>();
    }
    void Update()
    {
        FindInput();
    }

    //returns false if it failed to find a controller to pass to
    private bool FindInput()
    {
        //listen for input
        string[] input = new string[3];
        //passes the controller number and button and intensity if applicable ie the triggers
        //if its onpress or onrelease maybe if necessary
        //pass input to current states controller in the scene whatever it may be
        if(Input.GetAxis("Horizontal_a") != 0)
        {
            input[0] = "Controller_a";
            input[1] = "RightStick_X";
            input[2] = Input.GetAxis("Horizontal_a").ToString();
            controller.InputHandle(input);
        }
        if(Input.GetAxis("Vertical_a") != 0)
        {
            input[0] = "Controller_a";
            input[1] = "RightStick_y";
            input[2] = Input.GetAxis("Vertical_a").ToString();
            controller.InputHandle(input);
        }
        if(Input.GetAxis("A_a") != 0)
        {
            input[0] = "Controller_a";
            input[1] = "A_a";
            input[2] = Input.GetAxis("A_a").ToString();
            controller.InputHandle(input);
        }
        if(Input.GetAxis("R3_a") != 0)
        {
            input[0] = "Controller_a";
            input[1] = "R3_a";
            input[2] = Input.GetAxis("R3_a").ToString();
            controller.InputHandle(input);
        }

        return true;
    }
}