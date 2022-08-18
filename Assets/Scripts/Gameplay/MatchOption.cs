using System;

namespace Gameplay
{
    public interface IMatchOption
    {
        int PlayerNum { get; }
        int TilesPerGroup { get; }
        event Action OnChanged;
    }

    public class MatchOption : IMatchOption, IManager
    {
        public int PlayerNum { get; private set; } = 3;
        public int TilesPerGroup { get; private set; } = 5;
        public event Action OnChanged;
        
        public void OnInitialize()
        {
            
        }

        public void OnSetup()
        {
            PlayerNum = 3;
            TilesPerGroup = 5;
            OnChanged?.Invoke();
        }

        public void OnGameStart()
        {
        }

        public void OnGameEnd()
        {
        }

        public void OnGameReset()
        {
        }

        public void OnCleanup()
        {
        }
    }
}