using Gameplay.Board;
using UnityEngine;

namespace Gameplay.Piece
{
    public class CitizenToTileSelectorAdaptor : TileSelector.ISelectionAdaptor
    {
        private readonly Citizen _piece;

        public CitizenToTileSelectorAdaptor(Citizen piece)
        {
            _piece = piece;
        }

        public void OnTileSelected()
        {
            _piece.PlayAnimStandUp();
        }

        public void OnTileDeselected(bool success)
        {
            if (!success)
            {
                _piece.PlayAnimSitDown();
            }
        }
    }
}