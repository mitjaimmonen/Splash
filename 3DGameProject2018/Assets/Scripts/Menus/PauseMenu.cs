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



    #region Public Functions

    /// <summary>
    /// Resumes game from pausing
    /// </summary>
    public void Resume() {

        controller.IsPaused = false;
        gameObject.SetActive(false);
    }
    /// <summary>
    /// State change to main menu
    /// </summary>
    public void MainMenu() {
        controller.IsPaused = false;
        gameObject.SetActive(false);
        controller.StateHandler.ChangeState(State.MainMenu);
    }
    /// <summary>
    /// Quits Game to desktop
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }

    #endregion

}
