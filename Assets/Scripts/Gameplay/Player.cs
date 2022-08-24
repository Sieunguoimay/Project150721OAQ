using System;
using System.Linq;
using Common.ResolveSystem;
using Gameplay.Board;
using Gameplay.Piece;
using SNM;
using UnityEngine;

namespace Gameplay
{
    [Serializable]
    public abstract class Player
    {
        protected TileSelector TileSelector { get; private set; }
        public event Action<Tile, bool> OnDecisionResult = delegate { };

        public PieceBench PieceBench { get; set; }
        public int Index { get; private set; }

        protected Player(int index, TileSelector tileSelector)
        {
            Index = index;
            TileSelector = tileSelector;
        }

        public virtual void ResetAll()
        {
            TileSelector.ChooseDirectionResult -= InvokeOnDecisionResult;
            PieceBench.Pieces.Clear();
        }

        public virtual void MakeDecision(Board.Board board)
        {
            TileSelector.Display(board.TileGroups[Index].Tiles.Select(t => t as ISelectorTarget).ToArray());
        }

        protected virtual void InvokeOnDecisionResult(ISelectorTarget selectorTarget, bool forward) =>
            OnDecisionResult?.Invoke(selectorTarget as Tile, forward);

        public virtual void ReleaseTurn()
        {
            TileSelector.ChooseDirectionResult -= InvokeOnDecisionResult;
        }

        public virtual void AcquireTurn()
        {
            TileSelector.ChooseDirectionResult += InvokeOnDecisionResult;
        }
    }

    public class RealPlayer : Player
    {
        public RealPlayer(int index, TileSelector tileSelector) : base(index, tileSelector)
        {
        }

        public override void MakeDecision(Board.Board board)
        {
            base.MakeDecision(board);
            foreach (var t in board.TileGroups[Index].Tiles)
            {
                ((Tile) t).OnTouched -= TileSelector.SelectTile;
                ((Tile) t).OnTouched += TileSelector.SelectTile;
            }

            TileSelector.DirectionTouched -= DirectionTileSelectorTouched;
            TileSelector.DirectionTouched += DirectionTileSelectorTouched;
            TileSelector.Display(board.TileGroups[Index].Tiles.Select(t => t as ISelectorTarget).ToArray());
        }

        private void DirectionTileSelectorTouched(bool direction)
        {
            TileSelector.ChooseDirection(direction);
            TileSelector.DirectionTouched -= DirectionTileSelectorTouched;
        }
    }

    public class FakePlayer : Player
    {
        private Coroutine _coroutine;

        public FakePlayer(int index, TileSelector tileSelector) : base(index, tileSelector)
        {
        }

        public override void ResetAll()
        {
            base.ResetAll();
            if (_coroutine != null)
            {
                TileSelector.StopCoroutine(_coroutine);
                _coroutine = null;
            }
        }

        public override void MakeDecision(Board.Board board)
        {
            base.MakeDecision(board);

            IPieceHolder selectedTile = null;
            var selectedDirection = UnityEngine.Random.Range(0, 100f) > 50f;

            foreach (var t in board.TileGroups[Index].Tiles.Where(t => t.Pieces.Count > 0))
            {
                selectedTile = t;
                if (UnityEngine.Random.Range(0, 100f) > 50f)
                {
                    break;
                }
            }

            TileSelector.SelectTile(selectedTile as Tile);

            _coroutine = TileSelector.Delay(.4f, () =>
            {
                TileSelector.ChooseDirection(selectedDirection);
                _coroutine = null;
            });
        }
    }
}