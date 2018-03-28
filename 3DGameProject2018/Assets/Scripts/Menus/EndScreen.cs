using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndScreen : MonoBehaviour, IController {
    public GameObject visualStatPrefab;
    private StateHandler state;
    private List<WinCards> InstatntiatedStats = new List<WinCards>();
    public RectTransform canvas;

    //instantiate visual stats and fill them with the needed data
    private void Start()
    {
        state = GameObject.FindGameObjectWithTag("State Handler").GetComponent<StateHandler>();
        state.Sort();
        for(int i = 0; i < state.stats.Count; i++)
        {

            GameObject temp = Instantiate(visualStatPrefab, transform);
            temp.GetComponent<RectTransform>().anchoredPosition = new Vector3((canvas.rect.width / state.stats.Count) * (i + 1) - ((canvas.rect.width / state.stats.Count) / 2), 20, 0);
            InstatntiatedStats.Add(temp.GetComponent<WinCards>());// needs to instantiate at differnet locations
            InstatntiatedStats[i].player.text = "Player: "+(state.stats[i].player+1);
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
        state.stats.Clear();
    }
    //make a switch to start the match again
    public void Restart()
    {
        state.ChangeState(State.Game);
        state.stats.Clear();
    }

    public void InputHandle(string[] input)
    {
        
    }
}
