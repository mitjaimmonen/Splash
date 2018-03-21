using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour {


    public MatchController controller;

    public void Resume() {

        controller.isPaused = false;
        gameObject.SetActive(false);
    }

    public void MainMenu() {
        controller.isPaused = false;
        gameObject.SetActive(false);
        controller.stateHandler.ChangeState(State.MainMenu);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
