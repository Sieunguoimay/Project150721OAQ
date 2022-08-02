using System;
using System.Linq;
using SNM;

namespace Gameplay
{
    [Serializable]
    public class Player
    {
        public event Action<Tile, bool> OnDecisionResult = delegate { };

        public Board.TileGroup TileGroup { get; }
        public PieceBench PieceBench { get; }

        public Player(Board.TileGroup tileGroup, PieceBench pieceBench)
        {
            TileGroup = tileGroup;
            PieceBench = pieceBench;
        }

        public virtual void MakeDecision(Board board)
        {
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

            board.Delay(1f, () => { InvokeOnDecisionResult(selectedTile, selectedDirection); });
        }

        protected virtual void InvokeOnDecisionResult(Tile arg1, bool arg2) => OnDecisionResult?.Invoke(arg1, arg2);

        public virtual void ReleaseTurn()
        {
        }

        public virtual void AcquireTurn()
        {
        }
    }

    public class RealPlayer : Player
    {
        private readonly TileSelector _tileSelector;

        public RealPlayer(Board.TileGroup tileGroup, PieceBench pieceBench, TileSelector tileSelector)
            : base(tileGroup, pieceBench)
        {
            _tileSelector = tileSelector;
        }

        public override void MakeDecision(Board board)
        {
            _tileSelector.Display(TileGroup);
        }

        public override void AcquireTurn()
        {
            _tileSelector.OnDone = InvokeOnDecisionResult;
        }
    }
}