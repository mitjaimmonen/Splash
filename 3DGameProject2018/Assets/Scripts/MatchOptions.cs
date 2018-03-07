public enum GameMode
{
    DeathMatch, TimedDeathMatch
}
public enum Map
{
    Map1
}
public class MatchOptions {

    public GameMode mode = GameMode.DeathMatch;
    private int currentActivePlayers = 0;
    //[[controller #, team, active]]
    private int[,] playerInfo = { {0,0,0 },{ 1, 1, 0 },{ 2, 2, 0 },{ 3, 3, 0 } };
    public Map map = Map.Map1;

    public int[,] PlayerInfo
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
    /// Enables the player at passed index and increments the players if the player wasn't already enabled.
    /// </summary>
    /// <param name="index"></param>
    public void EnablePlayer(int index) {
        if(PlayerInfo[index, 2] == 0)
        {
            PlayerInfo[index, 2] = 1;
            CurrentActivePlayers++;
        } 
    }

    /// <summary>
    /// Disables the player at passed index and decrements the players if the player wasn't already disabled.
    /// </summary>
    /// <param name="index"></param>
    public void DisablePlayer(int index)
    {
        if(PlayerInfo[index, 2] == 1)
        {
            PlayerInfo[index, 2] = 0;
            CurrentActivePlayers--;
        }
    }
}
