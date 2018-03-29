using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/********************************************
 * MatchController class
 *  Handles pausing spawning players and ending 
 *  the match
 */
public class MatchController : MonoBehaviour, IController{

    /******************/
    /*Member Variables*/
    public GameObject playerPrefab;
    public GameObject[] playerSpawns;
    public LayerMask PlayerLayerMask;
    public PauseMenu pauseMenu;
    private bool isPaused = false;
    private PlayerController[] instantiatedPlayers = new PlayerController[4];// only public for testing
    private StateHandler stateHandler;
    private float startTime;
    [SerializeField, Tooltip("Does match end on time limit")]
    private bool isTimed = false;
    [SerializeField, Tooltip("Time in seconds")]
    private float gameLength;
    private float gameTimer;
    [SerializeField, Tooltip("Does match end at maxKills")]
    private bool hasKillLimit = false;
    [SerializeField]
    private int maxKills = 1;



    #region Getters and Setters

    public bool IsPaused
    {
        get {
            return isPaused;
        }

        set {
            isPaused = value;
        }
    }
    public StateHandler StateHandler
    {
        get {
            return stateHandler;
        }

        private set {
            stateHandler = value;
        }
    }

    #endregion



    /************************/
    /*MonoBehavior Functions*/
    //Instantiate players and adjust controller options
    void Awake()
    {
        StateHandler = GameObject.FindGameObjectWithTag("State Handler").GetComponent<StateHandler>();
        playerPrefab.GetComponent<PlayerController>().currentPlayers = StateHandler.options.CurrentActivePlayers;
        
        //Instantiate players
        for(int i = 0; i < instantiatedPlayers.Length; i++)
        {
            if(StateHandler.options.PlayersInfo[i,2] ==1)
            {
                playerPrefab.GetComponent<PlayerController>().playerNumber = i;
                instantiatedPlayers[i] = Instantiate(playerPrefab).GetComponent<PlayerController>();
                instantiatedPlayers[i].Controller = this;
                Spawn(i);
            }
        }
        //set our exit conditions based on the statehandler.options
        if(StateHandler.options.maxKills != 0)
        {
            hasKillLimit = true;
            maxKills = (int)StateHandler.options.maxKills;
        }
        if(StateHandler.options.maxTime != 0)
        {
            isTimed = true;
            gameLength = 60*StateHandler.options.maxTime;//multiply by 60 to convert seconds to minutes
        }
    }
    //Check for our exit conditions
    public void Update()
    {
        if(!isPaused)
        {
            if(hasKillLimit)
            {
                for(int i = 0; i < StateHandler.options.CurrentActivePlayers; i++)
                {
                    if(instantiatedPlayers[i].stats.kills >= maxKills)
                    {
                        EndMatch();
                        break;
                    }

                }
            }
            if(isTimed)
            {
                gameTimer += Time.deltaTime;
                if(gameTimer >= gameLength)
                {
                    EndMatch();
                }
            }
        }
    }



    /*****************/
    /*Implementations*/
    //Sends input to player, and calls pausing
    public void InputHandle(string[] input)
    {
        //all input goes to the controllers appropriate player if active
        if(StateHandler.options.PlayersInfo[int.Parse(input[0]), 2] == 1 && !IsPaused)
        {
            if(input[1] == "Start")
            {
                Pause();
            } else
            {
                instantiatedPlayers[int.Parse(input[0])].InputHandle(input);
            }
        }
    }



    #region Public Functions
    
    /// <summary>
    /// Activates pause, and sets pause menu active
    /// </summary>
    public void Pause()
    {
        IsPaused = true;
        pauseMenu.gameObject.SetActive(true);
    }
    /// <summary>
    /// Respawns a given player at random unoccupied spawn
    /// </summary>
    /// <param name="playerIndex"></param>
    public void Spawn(int playerIndex)
    {
        List<GameObject> unocupiedSpawns = new List<GameObject>();

        for(int i = 0; i < playerSpawns.Length; i++)
        {
            if(!Physics.CheckSphere(playerSpawns[i].transform.position, 1, PlayerLayerMask))
            {
                unocupiedSpawns.Add(playerSpawns[i]);
            }
        }
        int spawn = Random.Range(0, unocupiedSpawns.Count);
        instantiatedPlayers[playerIndex].transform.position = playerSpawns[spawn].transform.position;
        instantiatedPlayers[playerIndex].transform.rotation = playerSpawns[spawn].transform.rotation;
        instantiatedPlayers[playerIndex].Reset();
    }

    #endregion



    #region Private Functions

    /// <summary>
    /// Gives player stats to StateHandler and calls scene change to EndMenu
    /// </summary>
    private void EndMatch() {
        for(int i = 0; i < StateHandler.options.CurrentActivePlayers; i++)
        {
            StateHandler.stats.Add(instantiatedPlayers[i].stats);
        }
        StateHandler.ChangeState(State.EndMenu);
    }

    #endregion

}