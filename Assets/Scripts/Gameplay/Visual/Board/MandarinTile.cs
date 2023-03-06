using System.Linq;
using Gameplay.Visual.Piece;

namespace Gameplay.Visual.Board
{
    public class MandarinTile : Tile
    {
        public override int GetNumTakenGridCells()
        {
            if (HasMandarin)
            {
                return base.GetNumTakenGridCells() + 9;
            }

            return base.GetNumTakenGridCells();
        }

        private bool HasMandarin => HeldPieces.Any(p => p is Mandarin);
    }
}