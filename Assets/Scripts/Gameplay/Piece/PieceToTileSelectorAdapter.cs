using Gameplay.Board;
using UnityEngine;

namespace Gameplay.Piece
{
    public class CitizenToTileSelectorAdaptor : ISelectionAdaptor
    {
        private readonly Piece _piece;

        public CitizenToTileSelectorAdaptor(Piece piece)
        {
            _piece = piece;
        }

        public void OnTileSelected()
        {
            if (_piece is Citizen c)
                c.PlayAnimStandUp();
        }

        public void OnTileDeselected(bool success)
        {
            if (!success)
            {
                if (_piece is Citizen c)
                    c.PlayAnimSitDown();
            }
        }
    }
}