using System;

namespace Gameplay.GameState
{
    public interface IGameState
    {
        GameState.State CurrentState { get; }
        event Action<GameState> StateChangedEvent;
    }

    public class GameState : IGameState
    {
        public State CurrentState { get; private set; }

        public void SetState(State newState)
        {
            CurrentState = newState;
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