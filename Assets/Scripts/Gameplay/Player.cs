using System;
using System.Linq;
using Gameplay.Board;
using SNM;

namespace Gameplay
{
    [Serializable]
    public abstract class Player
    {
        protected TileSelector TileSelector { get; private set; }
        public event Action<Tile, bool> OnDecisionResult = delegate { };

        public Board.Board.TileGroup TileGroup { get; }
        public PieceBench PieceBench { get; }

        protected Player(Board.Board.TileGroup tileGroup, PieceBench pieceBench, TileSelector tileSelector)
        {
            TileGroup = tileGroup;
            PieceBench = pieceBench;
            TileSelector = tileSelector;
        }

        public virtual void MakeDecision(Board.Board board)
        {
            TileSelector.Display(TileGroup);
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
        public RealPlayer(Board.Board.TileGroup tileGroup, PieceBench pieceBench, TileSelector tileSelector) : base(tileGroup, pieceBench, tileSelector)
        {
        }

        public override void MakeDecision(Board.Board board)
        {
            base.MakeDecision(board);
            foreach (var t in TileGroup.Tiles)
            {
                t.OnTouched -= TileSelector.SelectTile;
                t.OnTouched += TileSelector.SelectTile;
            }

            TileSelector.OnTouched += OnTileSelectorTouched;
            TileSelector.Display(TileGroup);
        }

        private void OnTileSelectorTouched(bool direction)
        {
            TileSelector.ChooseDirection(direction);
            TileSelector.OnTouched -= OnTileSelectorTouched;
        }
    }

    public class FakePlayer : Player
    {
        public FakePlayer(Board.Board.TileGroup tileGroup, PieceBench pieceBench, TileSelector tileSelector) : base(tileGroup, pieceBench, tileSelector)
        {
        }

        public override void MakeDecision(Board.Board board)
        {
            base.MakeDecision(board);

            Tile selectedTile = null;
            var selectedDirection = UnityEngine.Random.Range(0, 100f) > 50f;

            foreach (var t in TileGroup.Tiles.Where(t => t.Pieces.Count > 0))
            {
                selectedTile = t;
                if (UnityEngine.Random.Range(0, 100f) > 50f)
                {
                    break;
                }
            }

            TileSelector.SelectTile(selectedTile);

            board.Delay(.4f, () => { TileSelector.ChooseDirection(selectedDirection); });
        }
    }
}