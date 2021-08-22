public class PerMatchData
{
    private int[] playerScores;
    public int[] PlayerScores => playerScores;

    public PerMatchData(int playerNum)
    {
        playerScores = new int[playerNum];
    }

    public void SetPlayerScore(int playerIndex, int score)
    {
        PlayerScores[playerIndex] = score;
    }
}