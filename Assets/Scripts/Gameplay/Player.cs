﻿using System;
using System.Linq;
using Common.ResolveSystem;
using Gameplay.Board;
using Gameplay.Piece;
using SNM;

namespace Gameplay
{
    [Serializable]
    public abstract class Player
    {
        protected TileSelector TileSelector { get; private set; }
        public event Action<Tile, bool> OnDecisionResult = delegate { };

        public PieceBench PieceBench { get; set; }
        public int Index { get; private set; }

        protected Player(int index)
        {
            Index = index;
            // TileSelector = Resolver.Instance.Resolve<TileSelector>();
        }

        public virtual void MakeDecision(Board.Board board)
        {
            TileSelector.Display(board.TileGroups[Index]);
        }

        protected virtual void InvokeOnDecisionResult(Tile arg1, bool arg2) => OnDecisionResult?.Invoke(arg1, arg2);

        public virtual void ReleaseTurn()
        {
        }

        public virtual void AcquireTurn()
        {
            TileSelector.OnDone = InvokeOnDecisionResult;
        }
    }

    public class RealPlayer : Player
    {
        public RealPlayer(int index) : base(index)
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

            TileSelector.OnTouched += OnTileSelectorTouched;
            TileSelector.Display(board.TileGroups[Index]);
        }

        private void OnTileSelectorTouched(bool direction)
        {
            TileSelector.ChooseDirection(direction);
            TileSelector.OnTouched -= OnTileSelectorTouched;
        }
    }

    public class FakePlayer : Player
    {
        public FakePlayer(int index) : base(index)
        {
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

            TileSelector.Delay(.4f, () => { TileSelector.ChooseDirection(selectedDirection); });
        }
    }
}