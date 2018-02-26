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
    public int controllerAmount = 0;

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
        string[] input = new string[3];
        //listen for input
        for(int i = 0; i < controllerAmount; i++)
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
                Debug.Log("trip");
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
        return true;
    }
}