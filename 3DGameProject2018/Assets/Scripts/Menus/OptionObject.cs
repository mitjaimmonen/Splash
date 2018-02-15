using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum OptionType
{
    List, Slider, Value, button
}
//this is gonna go all options in our menus
public class OptionObject : MonoBehaviour {


    //all the settings for the option
    public OptionType type;
    public string label;
    public int value = 100;
    [SerializeField]
    private int min = 0;
    [SerializeField]
    private int max = 100;
    public List<string> listItems;


    //trackers
    private int currentIndex;
    //private float deltatime; //this is for if we do streaks making the incrementation larger


    //prefabs for all the menu types
    //gonna have to put these in for each option wich sucks but couldnt think of a simpler way rip
    public GameObject listPrefab;
    public GameObject sliderPrefab;
    public GameObject valuePrefab;
    public GameObject buttonPrefab;


    //if it isnt a list clear it to prevent breaking the code with an error
    //also get the text element in the child or however we set it up and give it the label
    //spawn the prefab for our current option type
    //set the appropriate types to their default value and min maxs
    private void Start()
    {
        if(type != OptionType.List)
        {
            listItems.Clear();
            //maybe send a debug warning too
        }
    }



    //return a string value of the current object
    //if list return list element of currentindex
    //slider or value just return the index/get the value of the fields
    //      this depends how its implemented idk if unity has actual slider input fields, also im not even sure if we will have raw inputs but im just trying to 
    //      give the option if we want it
    //button returns name
    public string GetValue()
    {
        return null;
    }



    //we could also do a thing were the increment goes up if its been held down but we can worry about that later
    //Increase index by one
    public void Increase()
    {

    }
    //decrease index by one
    public void Decrease()
    {

    }



    //Highlight an object
    public void Highlight(GameObject obj)
    {

    }
    //return an object to the normal state
    public void Normalize(GameObject obj)
    {

    }
}
