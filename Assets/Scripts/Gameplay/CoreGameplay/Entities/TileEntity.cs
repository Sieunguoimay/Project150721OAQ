using System.Collections.Generic;

namespace Gameplay.CoreGameplay.Entities
{
    public class PieceContainerEntity
    {
        public List<PieceEntity> PieceEntities;
    }

    public class TileEntity : PieceContainerEntity
    {
        public TileType TileType;
    }

    public enum TileType
    {
        CitizenTile,
        MandarinTile
    }
}