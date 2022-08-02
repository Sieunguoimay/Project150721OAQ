namespace System
{
    public class PerMatchData
    {
        public int[] PlayerScores { get; }

        public PerMatchData(int playerNum)
        {
            PlayerScores = new int[playerNum];
        }

        public void SetPlayerScore(int playerIndex, int score)
        {
            PlayerScores[playerIndex] = score;
        }
    }
}