using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using XInputDotNetPure;
public class MeinMenuHandler : MonoBehaviour, IController
{

    public FromToNode nodeController;
    public Node currentNode;
    EventSystem myEventSystem;
    public Node joinNode;
    private MatchOptions options = new MatchOptions();
    private StateHandler stateHandler;
    public GameObject[] players;
    private void Start()
    {
        stateHandler = GameObject.FindGameObjectWithTag("State Handler").GetComponent<StateHandler>();
        myEventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
        stateHandler.options.maxTime = 0;
        stateHandler.options.maxKills = 0;

    }

    private void Update()
    {
        for(int i = 0; i < 6; i++)
        {
            if((GamePad.GetState((PlayerIndex)i).Buttons.Start == ButtonState.Pressed || GamePad.GetState((PlayerIndex)i).Buttons.A == ButtonState.Pressed) && currentNode == joinNode)//if(Input.GetAxis("Joy" + i + "Start") != 0)
            {
                AddPlayer(i);
            }else if(GamePad.GetState((PlayerIndex)i).Buttons.B == ButtonState.Pressed)
            {
                BackNode();
            }
        }
    }
    /// <summary>
    /// Binds controller to an activated player
    /// </summary>
    /// <param name="controller">Xinput number of controller</param>
    private void AddPlayer(int controller)
    {
        if(stateHandler.options.EnablePlayer(controller))
        {
            int currentplayer = stateHandler.options.PlayerFromController(controller);
            players[currentplayer].GetComponent<AnimSelector>().Activate();
            
        }
    }
    /// <summary>
    /// Deactivates player
    /// </summary>
    /// <param name="controller">Xinput number of controller</param>
    private void RemovePlayer(int controller)
    {
        int currentplayer = stateHandler.options.PlayerFromController(controller);
        if(stateHandler.options.DisablePlayer(controller))
        {
            players[currentplayer].SetActive(false);
        }
    }
    public void BackNode() {
        if(currentNode.backNode != null && nodeController.StartTransition(currentNode.backNode, currentNode.backNodeSpeed))
        {
            currentNode = currentNode.backNode;
            myEventSystem.SetSelectedGameObject(null);
        }
    }
    public void NextNode() {
        if(nodeController.StartTransition(currentNode.nextNode, currentNode.nextNodeSpeed))
        {
            myEventSystem.SetSelectedGameObject(null);
        }
    }

    public void Exit() {
        Application.Quit();
        
    }
    public void SetFocus(GameObject focus) {
        myEventSystem.SetSelectedGameObject(focus);
    }
    public void InputHandle(string[] input)
    {
        if(input[1] == "Y" && currentNode == joinNode)
        {
            RemovePlayer(stateHandler.options.PlayersInfo[int.Parse(input[0]), 3]);
        }
        if(input[1] == "Start" && stateHandler.options.CurrentActivePlayers > 0 && currentNode == joinNode)
        {
            NextNode();
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
}
