namespace Gameplay.CoreGameplay.Entities
{
    public class BoardEntity
    {
        // public BoardSide[] Sides;
        // public class BoardSide
        // {
        //     public PocketEntity Pocket;
        //     public TileEntity MandarinTile;
        //     public TileEntity[] CitizenTiles;
        // }
        
        public PocketEntity[] Pockets;
        public TileEntity[] MandarinTiles;
        public TileEntity[] CitizenTiles;
    }
}