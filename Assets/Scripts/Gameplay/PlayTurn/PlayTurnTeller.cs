using System;

namespace Gameplay.PlayTurn
{
    public interface IPlayTurnTeller
    {
        event Action<IPlayTurnTeller> TurnChangedEvent;
        public IPlayTurnData CurrentTurn { get; }
        public void NextTurn();
    }

    public class PlayTurnTeller : IPlayTurnTeller
    {
        private IPlayTurnData[] _turns;
        private int _turnIndex;
        public IPlayTurnData CurrentTurn { get; private set; }
        public event Action<IPlayTurnTeller> TurnChangedEvent;

        public void SetTurns(IPlayTurnData[] turns, int initialTurnIndex)
        {
            _turns = turns;
            _turnIndex = initialTurnIndex;
            TurnChangedEvent?.Invoke(this);
        }

        public void NextTurn()
        {
            _turnIndex = (_turnIndex + 1) % _turns.Length;
            CurrentTurn = _turns[_turnIndex];
            TurnChangedEvent?.Invoke(this);
        }
    }
}