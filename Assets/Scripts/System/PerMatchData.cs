using Gameplay.Entities;

namespace System
{
    public class PerMatchData 
    {
        public Currency[] PlayerScores { get; }

        public PerMatchData(int playerNum)
        {
            PlayerScores = new Currency[playerNum];
        }

        public void SetPlayerScore(int playerIndex, int score)
        {
            PlayerScores[playerIndex].Set(score);
        }
    }
}