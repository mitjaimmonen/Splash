using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class CafeController : MonoBehaviour {


    /******************/
    /*Member Variables*/
    private StateHandler stateHandler;
    public CameraMover mover;
    public MovementNode currentNode;
    public MovementNode joinNode;
    public GameObject matchOptions;
    public EventSystem eventsys;
    public GameObject[] PlayerModels;
    /************************/
    /*MonoBehavior Functions*/
    //shows the currently active players and starts music
    private void Start()
    {
        stateHandler = GameObject.FindGameObjectWithTag("State Handler").GetComponent<StateHandler>();
        stateHandler.options.maxTime = 0;
        stateHandler.options.maxKills = 0;
    }
    private void Update()
    {
        if(!mover.isMoving)
        {
            for(int i = 0; i < 6; i++)
            {
                if(GamePad.GetState((PlayerIndex)i).Buttons.B == ButtonState.Pressed
                    && !mover.isMoving
                    && currentNode.backNode != null)
                {
                    ToNode(currentNode.backNode.gameObject);
                }
                if(currentNode.nodeNumber == joinNode.nodeNumber)
                {
                    if(GamePad.GetState((PlayerIndex)i).Buttons.A == ButtonState.Pressed)
                    {
                        if(stateHandler.options.EnablePlayer(i))
                        {
                            int currentplayer = stateHandler.options.PlayerFromController(i);
                            PlayerModels[currentplayer].SetActive(true);
                        }
                    }
                    if(GamePad.GetState((PlayerIndex)i).Buttons.Start == ButtonState.Pressed && stateHandler.options.CurrentActivePlayers >= 1)
                    {
                        ToNode(matchOptions);
                    }
                    if(GamePad.GetState((PlayerIndex)i).Buttons.Back == ButtonState.Pressed)
                    {

                    }
                }
            }
        }

    }


    /*****************/
    /*Implementations*/
    //Icontroller implementation, listens to exit a player
    public void InputHandle(string[] input)
    {
        if(input[1] == "Y")
        {
            int player = stateHandler.options.FindActiveNumber(int.Parse(input[0]));
            Debug.Log(int.Parse(input[0]));
            int count = 0;
            for(int i = 0; i < 4; i++)
            {
                if(PlayerModels[i].activeInHierarchy == true)
                {
                    count++;
                }
                if(count == int.Parse(input[0]) + 1)
                {
                    PlayerModels[i].SetActive(false);
                }
            }
            stateHandler.options.DisablePlayer(stateHandler.options.PlayersInfo[player, 3]);
        }
    }

    #region Public Functions


    public void ToNode(GameObject target)
    {
        if(!mover.isMoving)
        {
            MovementNode node = target.GetComponent<MovementNode>();
            currentNode = mover.ToNode(node.nodeNumber, node.moveTime);
            Focus(node.nodeFocus);
        }
    }

    private void Focus(GameObject focusobj)
    {
        if(focusobj != null)
        {
            eventsys.sendNavigationEvents = true;
            eventsys.SetSelectedGameObject(focusobj);
        } else
        {
            eventsys.sendNavigationEvents = false;
        }
    }

    public void StartGame()
    {
        //If there's an active player pass options to StateHandler and change state
        if(stateHandler.options.CurrentActivePlayers > 0)
        {
            stateHandler.ChangeState(State.Game);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    /// <summary>
    /// Change the max game time in options
    /// </summary>
    /// <param name="time">Slider that the value is pulled from</param>
    public void ChangeTime(Slider time)
    {
        stateHandler.options.maxTime = time.value;
    }
    /// <summary>
    /// Change the max kills in options
    /// </summary>
    /// <param name="time">Slider that the value is pulled from</param>
    public void ChangeKills(Slider kills)
    {
        stateHandler.options.maxKills = kills.value;
    }
    #endregion
}
