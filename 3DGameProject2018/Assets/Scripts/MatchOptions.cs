public enum GameMode
{
    DeathMatch, TimedDeathMatch
}
public enum Map
{
    Map1
}
/********************************************
 *  MatchOptions class
 * Class to hold all current match options
 * Such as controllers, teams, mode, and map
 */
public class MatchOptions {

    public GameMode mode = GameMode.DeathMatch;
    private int currentActivePlayers = 0;
    //[[player #, team, active, controller #]]
    private int[,] playerInfo = { {0, 0, 0, 0 },{ 1, 1, 0, 0 },{ 2, 2, 0, 0 },{ 3, 3, 0, 0 } };
    public Map map = Map.Map1;

    public int[,] PlayersInfo
    {
        get {
            return playerInfo;
        }

        private set {
            playerInfo = value;
        }
    }
    public int CurrentActivePlayers
    {
        get {
            return currentActivePlayers;
        }

        private set {
            currentActivePlayers = value;
        }
    }


    /// <summary>
    /// Enables the player of joy number and increments the players if the player wasn't already enabled.
    /// </summary>
    /// <param name="index"></param>
    /// <returns>Returns true if succesfull at enabling player</returns>
    public bool EnablePlayer(int joyNumber) {
        for(int i = 0; i < playerInfo.GetLength(0); i++)
        {
            if(PlayersInfo[i, 3] == joyNumber && PlayersInfo[i, 2] == 1)
            {
                return false;
            }
        }
        for(int i = 0; i < playerInfo.GetLength(0); i++)
        {
            if(PlayersInfo[i, 2] == 0)
            {
                PlayersInfo[i, 2] = 1;
                PlayersInfo[i, 3] = joyNumber;
                CurrentActivePlayers++;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Disables the player of joy number and decrements the players if the player isn't already disabled.
    /// </summary>
    /// <param name="index"></param>
    /// <returns>Returns true if succesfull at diasbling player</returns>
    public bool DisablePlayer(int joyNumber)
    {
        for(int i = 0; i < playerInfo.GetLength(0); i++)
        {
            if(PlayersInfo[i, 2] == 1 && PlayersInfo[i, 3] == joyNumber)
            {
                PlayersInfo[i, 2] = 0;
                CurrentActivePlayers--;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Finds the state of player at index passed
    /// </summary>
    /// <param name="index"></param>
    /// <returns>State of indexed player</returns>
    public bool IsPlayerActive(int index) {
        if(PlayersInfo[index, 2] == 1)
        {
            return true;
        }
        return false;
    }



    /// <summary>
    /// Get player info array of index player
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public int PlayerFromController(int controller) {
        for(int i = 0; i < playerInfo.GetLength(0); i++)
        {
            if(controller == PlayersInfo[i, 3]&& PlayersInfo[i, 2] == 1)
            {
                return i;
            }
        }
        return 0;
    }
}


