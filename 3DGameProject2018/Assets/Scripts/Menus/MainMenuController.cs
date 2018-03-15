using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour, IController
{
    private MatchOptions options = new MatchOptions();
    public TextMeshProUGUI[] visualPlayerElements = new TextMeshProUGUI[4];

    //stats to track were you are in both menues and a list of prefabs for each option
    private bool inSettings = false;

    private int currentSelectionMenu = 0;
    public OptionObject[] menuObjects;

    private int currentSelectionOptions = 0;
    public GameObject menuPrefab;
    public GameObject[] optionsObjects;
    private StateHandler stateHandler;
    //temp for player count
    [SerializeField, Tooltip("Number of players to initiate the game with")]
    private int players = 1;

    //if the options are prefabs initialize,or we could probably just have them placed in scene already through the editor
    //Highlight first element and set each input to the default option also assigning that in the match options
    //start main music at current saved settings volume
    private void Start()
    {
        stateHandler = GameObject.FindGameObjectWithTag("State Handler").GetComponent<StateHandler>();
        for(int i = 0; i < stateHandler.options.PlayersInfo.GetLength(0); i++)
        {
            if(stateHandler.options.PlayersInfo[i,2]==1)
            {

                visualPlayerElements[i].text = "Player " + (i+1) + " Ready";
            }
        }

    }


    /// <summary>
    /// Call state change to game
    /// Only works if theres active players
    /// </summary>
    public void StartGame()
    {
        //If there's an active player pass options to StateHandler and change state
        if(stateHandler.options.CurrentActivePlayers >0)
        {
            stateHandler.ChangeState(State.Game);
        }
    }



    private void Update()
    {
        for(int i = 0; i < 6; i++)
        {
            if(Input.GetAxis("Joy" + i + "Start") != 0)
            {
                AddPlayer(i);
            }
        }
    }


    //if in settings handle input accordingly else handle input in main menu
    // eg up goes up down down, left or right changes the selection
    //if up or down add or subtract currentselection and normalize the old item hightlight the new item
    //only accept input from a controller thats been added to team
    //also handle pressing a button to call add to team or remove
    //should take an array with the first element being the controller number and the second the appropriate button
    //also any recognized input should have a slight sound effect, selections and joining should be different
    //for any option navigation ie left and right(or x/y button) movement call menu/settingsobject[currentselectionmenu/settings].optionsobject.increase/decrease
    //if x on the settings button call settings
    //if x on the start call start
    //if x on settings menu exit then save settings
    public void InputHandle(string[] input)
    {
        if(input[1] == "Y")
        {
            RemovePlayer(stateHandler.options.PlayersInfo[int.Parse(input[0]), 3]);
        }

        //if(input[1] == "Start" && !stateHandler.options.IsPlayerActive(int.Parse(input[0])))
        //{
        //    stateHandler.options.EnablePlayer(int.Parse(input[0]));
        //    visualPlayerElements[int.Parse(input[0])].text = "Player " + input[0] + " Ready";
        //}
    }

    

    //add item to the list of players with the number of controller added
    //and visually update a players presence (update the playerpresence prefabs text and color to active aand team color)
    //when innitialized everyone will be on seperate teams 1-4
    //if the player is already in the array cycle its team up one(change its physical color to reflect)
    private void AddPlayer(int controller)
    {
        
        if(stateHandler.options.EnablePlayer(controller))
        {
            int currentplayer = stateHandler.options.PlayerFromController(controller);
            visualPlayerElements[currentplayer].text = "Player " + (currentplayer+1) + " Ready";
        }
    }
    //remove team element that has the controller being removed
    //shift all the players so they are left aligned
    private void RemovePlayer(int controller)
    {
        Debug.Log("herro");
        int currentplayer = stateHandler.options.PlayerFromController(controller);
        if(stateHandler.options.DisablePlayer(controller))
        {
            visualPlayerElements[currentplayer].text = "Press Start";
        }
    }



    //Brings up settings menu 
    //fades or puts an opaque object between options and menu
    //sets insettings to true
    private void ShowSettings()
    {

    }
    //takes settings stats, saves, and exit
    //adjust current musics and sound effects volume
    //insettings to false
    private void SaveSettings()
    {

    }
}
