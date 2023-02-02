using System.Collections.Generic;
using System.Linq;
using Gameplay.Piece;

namespace Gameplay.Board
{
    public interface ICitizenTile : ITile
    {
        IEnumerable<ISelectionAdaptor> GetSelectionAdaptors();
    }

    public class CitizenTile : Tile, ICitizenTile
    {
        public IEnumerable<ISelectionAdaptor> GetSelectionAdaptors() =>
            HeldPieces.Where(p => p is Citizen)
                .Select(p => new CitizenToTileSelectorAdaptor(p as Citizen));
    }
    
    public interface ISelectionAdaptor
    {
        void OnTileSelected();
        void OnTileDeselected(bool success);
    }
}