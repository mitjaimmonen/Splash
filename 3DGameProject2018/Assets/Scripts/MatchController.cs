using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/********************************************
 * MatchController class
 *  Handles pausing spawning players and ending 
 *  the match
 */
public class MatchController : MonoBehaviour, IController{

    /******************/
    /*Member Variables*/
    public GameObject playerPrefab;
    public LastMenuController lastmenu;
    public GameObject[] playerSpawns;
    public LayerMask PlayerLayerMask;
    public PauseMenu pauseMenu;
    public EventSystem es;
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
    private int respawnTime = 5;
    private float inputTimer = 0;
    private bool letInput = true;



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
    public int RespawnTime
    {
        get {return respawnTime;}
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

    public int GameCountdown
    {
        get {return (int)(gameLength-gameTimer);}

    }

    public int PlayerCount
    {
        get { return instantiatedPlayers.Length; }
    }

    #endregion



    /************************/
    /*MonoBehavior Functions*/
    //Instantiate players and adjust controller options
    void Awake()
    {
        Shader.WarmupAllShaders();
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
        } else
        {
            maxKills = 0;
            hasKillLimit = false;
        }
        if(StateHandler.options.maxTime != 0)
        {
            isTimed = true;
            gameLength = 60*StateHandler.options.maxTime;//multiply by 60 to convert seconds to minutes
        } else
        {
            gameLength = 0;
            isTimed = false;
        }
        respawnTime = stateHandler.options.respawnTime;
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
        if(letInput && StateHandler.options.PlayersInfo[int.Parse(input[0]), 2] == 1 )
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

    /*****************/
    /*   Coroutines  */
    IEnumerator DelayInput()
    {
        while(inputTimer <= .1f)
        {
            inputTimer += Time.deltaTime;
            yield return null;
        }
        Debug.Log(inputTimer);
        inputTimer = 0;
        letInput = true;
    }


    #region Public Functions

    /// <summary>
    /// Activates pause, and sets pause menu active
    /// </summary>
    public void Pause()
    {
        letInput = false;
        IsPaused = true;
        pauseMenu.gameObject.SetActive(true);
        es.SetSelectedGameObject(null);
        es.SetSelectedGameObject(es.firstSelectedGameObject);
    }
    /// <summary>
    /// Delays input for a few seconds then resumes
    /// </summary>
    public void Unpause()
    {
        StartCoroutine("DelayInput");
        IsPaused = false;
        pauseMenu.gameObject.SetActive(false);
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
            if(!Physics.CheckSphere(playerSpawns[i].transform.position, 5, PlayerLayerMask, QueryTriggerInteraction.Collide))
            {
                unocupiedSpawns.Add(playerSpawns[i]);
            }
        }
        Debug.Log(unocupiedSpawns.Count);
        int spawn = Random.Range(0, unocupiedSpawns.Count);
        instantiatedPlayers[playerIndex].transform.position = unocupiedSpawns[spawn].transform.position;
        instantiatedPlayers[playerIndex].transform.rotation = unocupiedSpawns[spawn].transform.rotation;
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

        stateHandler.controller = lastmenu;
        lastmenu.gameObject.SetActive(true);
        es.SetSelectedGameObject(null);
        es.SetSelectedGameObject(lastmenu.firstSelectedGameObject.gameObject);
        lastmenu.Finish();
        //StateHandler.ChangeState(State.EndMenu);
        Destroy(gameObject);
    }

    #endregion

}