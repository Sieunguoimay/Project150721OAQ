using System.Collections.Generic;
using System.Linq;
using Gameplay.Piece;

namespace Gameplay.Board
{
    public interface ICitizenTile : ITile
    {
    }

    public class CitizenTile : Tile, ICitizenTile
    {
    }
}