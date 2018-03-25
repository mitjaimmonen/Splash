using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndScreen : MonoBehaviour {
    public GameObject visualStatPrefab;
    private StateHandler state;
    private List<WinCards> InstatntiatedStats = new List<WinCards>();

    //instantiate visual stats and fill them with the needed data
    private void Start()
    {
        state = GameObject.FindGameObjectWithTag("State Handler").GetComponent<StateHandler>();
        
        state.Sort();
        for(int i = 0; i < state.stats.Count; i++)
        {
            InstatntiatedStats.Add(Instantiate(visualStatPrefab).GetComponent<WinCards>());// needs to instantiate at differnet locations
            InstatntiatedStats[i].gameObject.transform.position += new Vector3(15*i,0,0);
            InstatntiatedStats[i].player.text = "Player: "+state.stats[i].player;
            InstatntiatedStats[i].kills.text = "Kills: " + state.stats[i].kills;
            InstatntiatedStats[i].deaths.text = "Deaths: " + state.stats[i].deaths;
        }
        //need to make stats prefab script to change the text
        //go through the state.stats list and make the instantiatedstats text equal for every entry
    }

    //make a switch to main menu
    public void MainMenu()
    {
        state.ChangeState(State.MainMenu);
    }
    //make a switch to start the match again
    public void Restart()
    {
        state.ChangeState(State.Game);
    }
}
