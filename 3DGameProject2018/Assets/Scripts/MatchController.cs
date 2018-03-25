using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchController : MonoBehaviour, IController
{
    public int maxKills = 10;

    public GameObject playerPrefab;
    public GameObject[] playerSpawns;
    public GameObject[] weaponSpawns;
    public GameObject[] dropSpawns;
    private float startTime;
    public bool isPaused = false;
    private PlayerController[] instantiatedPlayers = new PlayerController[4];// only public for testing
    public StateHandler stateHandler;
    public LayerMask PlayerLayerMask;
    public PauseMenu pause_menu;


    //Initialize screens player and map
    void Awake()
    {
        
        stateHandler = GameObject.FindGameObjectWithTag("State Handler").GetComponent<StateHandler>();
        playerPrefab.GetComponent<PlayerController>().currentPlayers = stateHandler.options.CurrentActivePlayers;
        //make players of amount options player //not this right now(and pass each its input controller number)
        for(int i = 0; i < instantiatedPlayers.Length; i++)
        {
            if(stateHandler.options.PlayersInfo[i,2] ==1)
            {
                playerPrefab.GetComponent<PlayerController>().playerNumber = i;
                instantiatedPlayers[i] = Instantiate(playerPrefab).GetComponent<PlayerController>();
                instantiatedPlayers[i].playerNumber = i;
                instantiatedPlayers[i].Controller = this;
                Spawn(i);
                
            }

            
        }
                
        //adjust camera views for the current number of players
        //spawn players and add them to initializedplayers
    }



    public void Update()
    {
        if(stateHandler.options.mode == GameMode.DeathMatch)
        {
            for(int i = 0; i < stateHandler.players; i++)
            {
                if(instantiatedPlayers[i].stats.kills >= maxKills)
                {
                    EndMatch();
                    
                }
                
            }
        }
        //depending on game mode look for exit condition
        //ie time if not paused, or cycle players and look for a kill count
        //timed deathmatch will be fixxed for now but later we can add a condition if timed death is selected to add a slider option after for the duration of like 5 to 60 or something
        //also check all spawn objects and if theyve been ampty for x time spawn a new one
    }

    private void EndMatch() {
        for(int i = 0; i < stateHandler.players; i++)
        {
            stateHandler.stats.Add(instantiatedPlayers[i].stats);
        }
        stateHandler.ChangeState(State.EndMenu);
    }

    public void InputHandle(string[] input)
    {
        
        //all input goes to the controllers appropriate player if active
        if(stateHandler.options.PlayersInfo[int.Parse(input[0]), 2] == 1 && !isPaused)
        {
            if(input[1] == "Start")
            {
                Debug.Log(input[2]);
                Pause();
            } else
            {
                instantiatedPlayers[int.Parse(input[0])].InputHandle(input);
            }
        }
    }



    //takes the player position and will respawn that player at an unoccupied spawn
    public void Spawn(int playerIndex)
    {
        //right now just spawns at spawn point of same number we should make this more complex in the future
        for(int i = 0; i < playerSpawns.Length; i++)
        {
            if(!Physics.CheckSphere(playerSpawns[i].transform.position,1,PlayerLayerMask))
            {
                instantiatedPlayers[playerIndex].transform.position = playerSpawns[i].transform.position;
                instantiatedPlayers[playerIndex].transform.rotation = playerSpawns[i].transform.rotation;
                instantiatedPlayers[playerIndex].Reset();
                
                return;
            }
        }
    }


    //stops timer 
    //calls all players to fade screen
    //calls player that paused show pause menu
    public void Pause()
    {
        isPaused = true;
        pause_menu.gameObject.SetActive(true);
    }
}