using System;
using Gameplay.Board;
using Gameplay.GameInteract;

namespace Gameplay
{
    [Serializable]
    public class Player
    {
        private GameInteractManager _interactManager;
        private Action<Tile, bool> _onDecisionResult;
        public PieceBench PieceBench { get; set; }
        public int Index { get; private set; }

        public Player(int index, GameInteractManager interactManager)
        {
            Index = index;
            _interactManager = interactManager;
        }

        public virtual void ResetAll()
        {
            PieceBench.Pieces.Clear();
        }

        public virtual void MakeDecision(Board.Board board, Action<Tile, bool> onResult)
        {
            _onDecisionResult = onResult;
            _interactManager.PerformAction(board.TileGroups[Index], null, InvokeOnDecisionResult);
        }

        protected void InvokeOnDecisionResult((Tile, bool) obj)
        {
            var (item1, item2) = obj;
            _onDecisionResult?.Invoke(item1, item2);
        }

        public virtual void ReleaseTurn()
        {
        }

        public virtual void AcquireTurn()
        {
        }
    }
}