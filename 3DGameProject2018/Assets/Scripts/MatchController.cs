using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchController : MonoBehaviour, IController
{

    public MatchOptions options;// this is only public so we can pass an option us the map scene right away shouldnt be used otherwise
    public GameObject playerPrefab;
    public GameObject[] playerSpawns;
    public GameObject[] weaponSpawns;
    public GameObject[] dropSpawns;
    private float startTime;
    private bool isPaused = false;
    public PlayerController[] instantiatedPlayers;// only public for testing

    //Initialize screens player and map
    void Start()
    {
        //make players of amount options player and pass each its input controller number
        //adjust camera views for the current number of players
        //spawn players and add them to initializedplayers
    }



    public void Update()
    {
        //depending on game mode look for exit condition
        //ie time if not paused, or cycle players and look for a kill count
        //timed deathmatch will be fixxed for now but later we can add a condition if timed death is selected to add a slider option after for the duration of like 5 to 60 or something
        //also check all spawn objects and if theyve been ampty for x time spawn a new one
    }



    public void InputHandle(string[] input)
    {
        //all input goes to the controllers appropriate player
        switch(input[0])
        {
            case "0":
                instantiatedPlayers[0].InputHandle(input);
                break;
            case "1":
                instantiatedPlayers[1].InputHandle(input);
                break;
            case "2":
                instantiatedPlayers[2].InputHandle(input);
                break;
            case "3":
                instantiatedPlayers[3].InputHandle(input);
                break;
            default:
                break;
        }
    }



    //takes the player position and will respawn that player at an unoccupied spawn
    public void Spawn(int playerIndex)
    {

    }


    //stops timer 
    //calls all players to fade screen
    //calls player that paused show pause menu
    public void Pause(int player)
    {

    }
}