using System;

namespace Gameplay.PlayTurn
{
    public interface IPlayTurnTeller
    {
        event Action<IPlayTurnTeller> TurnChangedEvent;
        public PlayTurnData CurrentTurn { get; }
        public void NextTurn();
    }

    public class PlayTurnTeller : IPlayTurnTeller
    {
        private PlayTurnData[] _turns;
        private int _turnIndex;
        public PlayTurnData CurrentTurn { get; private set; }
        public event Action<IPlayTurnTeller> TurnChangedEvent;

        public void SetTurns(PlayTurnData[] turns, int initialTurnIndex)
        {
            _turns = turns;
            SetCurrentTurn(initialTurnIndex);
        }

        public void NextTurn()
        {
            var nextTurnIndex = (_turnIndex + 1) % _turns.Length;
            SetCurrentTurn(nextTurnIndex);
        }

        private void SetCurrentTurn(int index)
        {
            _turnIndex = index;
            CurrentTurn = _turns[_turnIndex];
            TurnChangedEvent?.Invoke(this);
        }
    }
}