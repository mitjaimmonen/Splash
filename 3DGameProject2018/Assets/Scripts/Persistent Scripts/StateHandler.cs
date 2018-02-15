using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum State
{
    MainMenu, Game, EndMenu
}
public class StateHandler : MonoBehaviour {

    private MatchOptions options;
    //we will search and find this during change state
    private GameObject controller;//this is gonna be empty in all but the initial menu

    //handle changing the state
    public void ChangeState(State desiredState) {
        //if main start main
        //if end start end
        //if game load scene of map
            //pass match controller the options and teams
    }

    void Update() {
        //call input parser
    }

    //returns false if it failed to find a controller to pass to
    private bool PassInput() {
        //listen for input
        //pass input to current states controller in the scene whatever it may be
        //passes the controller number and button and intensity if applicable ie the triggers
        //string[] 
        return true;
    }
}
