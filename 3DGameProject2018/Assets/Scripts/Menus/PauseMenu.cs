using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/********************************************
 * PauseMenu class
 *  Stops gameplay opens pause screen
 */
public class PauseMenu : MonoBehaviour {

    /******************/
    /*Member Variables*/
    public MatchController controller;



    /******************/
    /*Public Functions*/
    /// <summary>
    /// Resumes game from pausing
    /// </summary>
    public void Resume() {

        controller.isPaused = false;
        gameObject.SetActive(false);
    }
    /// <summary>
    /// State change to main menu
    /// </summary>
    public void MainMenu() {
        controller.isPaused = false;
        gameObject.SetActive(false);
        controller.stateHandler.ChangeState(State.MainMenu);
    }
    /// <summary>
    /// Quits Game to desktop
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }

}
