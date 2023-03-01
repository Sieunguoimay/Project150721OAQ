using System;
using Gameplay.Visual.Piece;

namespace Gameplay.Visual.Board
{
    public class MandarinTile : Tile
    {
        [field: NonSerialized] public Mandarin Mandarin { get; set; }

        public override int GetNumTakenGridCells()
        {
            return base.GetNumTakenGridCells() + (Mandarin != null ? 9 : 0);
        }
    }
}