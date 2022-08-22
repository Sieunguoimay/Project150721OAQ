using System;

namespace Gameplay
{
    public interface IMatchOption
    {
        int PlayerNum { get; }
        int TilesPerGroup { get; }
        event Action OnMatchOptionChanged;
    }

    public class MatchOption : IMatchOption
    {
        public int PlayerNum { get; private set; } = 3;
        public int TilesPerGroup { get; private set; } = 5;
        public event Action OnMatchOptionChanged;

        public void SetMatchOption(int playerNum, int tilesPerGroup)
        {
            PlayerNum = playerNum;
            TilesPerGroup = tilesPerGroup;
            OnMatchOptionChanged?.Invoke();
        }
    }
}