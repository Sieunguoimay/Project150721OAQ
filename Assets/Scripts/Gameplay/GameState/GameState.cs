using System;

namespace Gameplay.GameState
{
    public interface IGameState
    {
        GameState.State CurrentSate { get; }
        event Action<GameState> StateChangedEvent;
    }

    public class GameState : IGameState
    {
        public State CurrentSate { get; private set; }

        public void SetState(State newState)
        {
            CurrentSate = newState;
            StateChangedEvent?.Invoke(this);
        }

        public event Action<GameState> StateChangedEvent;

        public enum State
        {
            InMenu,
            Playing,
            Paused,
            Ended
        }
    }
}