using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchController : MonoBehaviour, IController
{

    public GameObject playerPrefab;
    public GameObject[] playerSpawns;
    public GameObject[] weaponSpawns;
    public GameObject[] dropSpawns;
    private float startTime;
    private bool isPaused = false;
    private PlayerController[] instantiatedPlayers = new PlayerController[4];// only public for testing
    private StateHandler stateHandler;



    //Initialize screens player and map
    void Start()
    {
        
        stateHandler = GameObject.FindGameObjectWithTag("State Handler").GetComponent<StateHandler>();
        playerPrefab.GetComponent<PlayerController>().currentPlayers = stateHandler.options.CurrentActivePlayers;
        //make players of amount options player //not this right now(and pass each its input controller number)
        for(int i = 0; i < instantiatedPlayers.Length; i++)
        {
            if(stateHandler.options.PlayerInfo[i,2] ==1)
            {
                playerPrefab.GetComponent<PlayerController>().playerNumber = i+1;
                instantiatedPlayers[i] = Instantiate(playerPrefab).GetComponent<PlayerController>();
                
            }
            
        }
        Spawn(0);
        Spawn(1);
        
        
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
        //all input goes to the controllers appropriate player if active
        if(stateHandler.options.PlayerInfo[int.Parse(input[0]), 2] == 1)
        {
            
            instantiatedPlayers[int.Parse(input[0])].InputHandle(input);
        }
    }



    //takes the player position and will respawn that player at an unoccupied spawn
    public void Spawn(int playerIndex)
    {
        //right now just spawns at spawn point of same number we should make this more complex in the future
        instantiatedPlayers[playerIndex].transform.position = playerSpawns[playerIndex].transform.position;
    }


    //stops timer 
    //calls all players to fade screen
    //calls player that paused show pause menu
    public void Pause(int player)
    {

    }
}