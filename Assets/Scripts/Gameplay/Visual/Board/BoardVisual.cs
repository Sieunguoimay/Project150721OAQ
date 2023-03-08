using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Visual.Board
{
    public class BoardSideVisual
    {
        public MandarinTile MandarinTile;
        public IReadOnlyList<CitizenTile> CitizenTiles;
    }

    public class BoardMetadata
    {
        public float TileSize;
        public int NumCitizenTilesPerSide;
        public IReadOnlyList<Vector2> Polygon;
    }

    public class BoardVisual : MonoBehaviour
    {
        public IReadOnlyList<Tile> Tiles { get; private set; }
        public IReadOnlyList<BoardSideVisual> SideVisuals { get; private set;}
        public BoardMetadata Metadata { get; private set;}

        public void SetReferences(IReadOnlyList<BoardSideVisual> sides, IReadOnlyList<Tile> tiles, BoardMetadata metadata)
        {
            SideVisuals = sides;
            Tiles = tiles;
            Metadata = metadata;
        }
    }
}