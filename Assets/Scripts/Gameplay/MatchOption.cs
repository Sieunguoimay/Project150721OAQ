using System;

namespace Gameplay
{
    public interface IMatchChooser
    {
        int PlayerNum { get; }
        int TilesPerGroup { get; }
        event Action OnMatchOptionChanged;
    }

    public class MatchChooser : IMatchChooser
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