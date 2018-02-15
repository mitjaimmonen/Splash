using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : MonoBehaviour {
    private MatchOptions options;
    private int[,] team;
    public GameObject[] visualPlayerElements = new GameObject[4];

    //stats to track were you are in both menues and a list of prefabs for each option
    private bool inSettings = false;
    private int currentSelectionMenu = 0;
    public GameObject[] menuObjects;
    private int currentSelectionOptions = 0;
    public GameObject menuPrefab;
    public GameObject[] optionsObjects;

    //if the options are prefabs initialize,or we could probably just have them placed in scene already through the editor
    //Highlight first element and set each input to the default option also assigning that in the match options
    //start main music at current saved settings volume
    private void Start()
    {
        
    }


    //update options to hold the current selections
    //pass options and team to the state handler along with ativating state change into the game
    private void StartGame()
    {
        //for menuobjects pass oprionobject.label, optionobject.getvalue to the current options
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
        
    }

    

    //add item to the list of players with the number of controller added
    //and visually update a players presence (update the playerpresence prefabs text and color to active aand team color)
    //when innitialized everyone will be on seperate teams 1-4
    //if the player is already in the array cycle its team up one(change its physical color to reflect)
    private void AddPlayer(int controller)
    {

    }
    //remove team element that has the controller being removed
    //shift all the players so they are left aligned
    private void RemovePlayer(int controller)
    {

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
