using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Visual.Board
{
    public class BoardSide
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

    public class Board : MonoBehaviour
    {
        public IReadOnlyList<Tile> Tiles { get; private set; }
        public IReadOnlyList<BoardSide> Sides { get; private set;}
        public BoardMetadata Metadata { get; private set;}

        public void SetReferences(IReadOnlyList<BoardSide> sides, IReadOnlyList<Tile> tiles, BoardMetadata metadata)
        {
            Sides = sides;
            Tiles = tiles;
            Metadata = metadata;
        }
    }
}