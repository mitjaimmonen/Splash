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
    public int players = 1;
    //all the teams need to be changed to a list later that way if we restart a match its mutable
    public int[,] Teams;
    public Map map = Map.Map1;

    public void InitializeTeams(int[,] teams) {
        Teams = teams;
    }


    //takes an option name runs it through a switch and changes it to the resultant option
    //this is gonna be clunky and probaably have to rely on lots of switch/if statememnts but i cant think of a better way
    //maybe if we used key value pairs instead of enums, but enums work good in unity editor side so ¯\_(ツ)_/¯
    //also its only called once on scene change so couldnt care less
    public void ChangeOption(string option, string value)
    {

    }
}
