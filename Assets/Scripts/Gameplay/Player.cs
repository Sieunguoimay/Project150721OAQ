using System;
using System.Linq;
using Common.ResolveSystem;
using Gameplay.Board;
using Gameplay.GameInteract;
using Gameplay.Piece;
using SNM;
using UnityEngine;

namespace Gameplay
{
    [Serializable]
    public class Player
    {
        // protected TileSelector TileSelector { get; private set; }
        protected GameInteractManager InteractManager;
        private Action<Tile, bool> _onDecisionResult;

        public PieceBench PieceBench { get; set; }
        public int Index { get; private set; }

        public Player(int index, GameInteractManager interactManager)
        {
            Index = index;
            // TileSelector = tileSelector;
            InteractManager = interactManager;
        }

        public virtual void ResetAll()
        {
            // TileSelector.ChooseDirectionResult -= InvokeOnDecisionResult;
            PieceBench.Pieces.Clear();
        }

        public virtual void MakeDecision(Board.Board board, Action<Tile, bool> onResult)
        {
            _onDecisionResult = onResult;
            // TileSelector.Display(board.TileGroups[Index].Tiles.Select(t => t as ISelectorTarget).ToArray());
            InteractManager.PerformAction(board.TileGroups[Index], InvokeOnDecisionResult);
        }

        protected void InvokeOnDecisionResult((Tile, bool) obj)
        {
            _onDecisionResult?.Invoke(obj.Item1, obj.Item2);
        }

        // protected virtual void InvokeOnDecisionResult(ISelectorTarget selectorTarget, bool forward) =>
        //     _onDecisionResult?.Invoke(selectorTarget as Tile, forward);

        public virtual void ReleaseTurn()
        {
            // TileSelector.ChooseDirectionResult -= InvokeOnDecisionResult;
        }

        public virtual void AcquireTurn()
        {
            // TileSelector.ChooseDirectionResult += InvokeOnDecisionResult;
        }
    }

    // public class RealPlayer : Player
    // {
    //     public RealPlayer(int index,  GameInteractManager interactManager) : base(index, interactManager)
    //     {
    //     }
    //
    //     public override void MakeDecision(Board.Board board, Action<Tile, bool> onResult)
    //     {
    //         base.MakeDecision(board, onResult);
    //         foreach (var t in board.TileGroups[Index].Tiles)
    //         {
    //             ((Tile) t).OnTouched -= TileSelector.SelectTile;
    //             ((Tile) t).OnTouched += TileSelector.SelectTile;
    //         }
    //
    //         TileSelector.DirectionTouched -= DirectionTileSelectorTouched;
    //         TileSelector.DirectionTouched += DirectionTileSelectorTouched;
    //         TileSelector.Display(board.TileGroups[Index].Tiles.Select(t => t as ISelectorTarget).ToArray());
    //     }
    //
    //     private void DirectionTileSelectorTouched(bool direction)
    //     {
    //         TileSelector.ChooseDirection(direction);
    //         TileSelector.DirectionTouched -= DirectionTileSelectorTouched;
    //     }
    // }

    // public class FakePlayer : Player
    // {
    //     private Coroutine _coroutine;
    //
    //     public FakePlayer(int index,  GameInteractManager interactManager) : base(index, interactManager)
    //     {
    //     }
    //
    //     public override void ResetAll()
    //     {
    //         base.ResetAll();
    //         if (_coroutine != null)
    //         {
    //             TileSelector.StopCoroutine(_coroutine);
    //             _coroutine = null;
    //         }
    //     }

        // public override void MakeDecision(Board.Board board, Action<Tile, bool> onResult)
        // {
        //     base.MakeDecision(board, onResult);
        //
        //     IPieceHolder selectedTile = null;
        //     var selectedDirection = UnityEngine.Random.Range(0, 100f) > 50f;
        //
        //     foreach (var t in board.TileGroups[Index].Tiles.Where(t => t.Pieces.Count > 0))
        //     {
        //         selectedTile = t;
        //         if (UnityEngine.Random.Range(0, 100f) > 50f)
        //         {
        //             break;
        //         }
        //     }
        //
        //     TileSelector.SelectTile(selectedTile as Tile);
        //
        //     _coroutine = TileSelector.Delay(.4f, () =>
        //     {
        //         TileSelector.ChooseDirection(selectedDirection);
        //         _coroutine = null;
        //     });
        // }
    // }
}