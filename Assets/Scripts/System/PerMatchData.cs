using Gameplay.Entities;

namespace System
{
    public class PerMatchData
    {
        public Currency[] PlayerScores { get; }

        public PerMatchData(int playerNum)
        {
            PlayerScores = new Currency[playerNum];
            for (var i = 0; i < PlayerScores.Length; i++)
            {
                PlayerScores[i] = new Currency();
            }
        }

        public void SetPlayerScore(int playerIndex, int score)
        {
            PlayerScores[playerIndex].Set(score);
        }
    }
}