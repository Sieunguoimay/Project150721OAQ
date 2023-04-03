using System.Collections.Generic;
using Gameplay.Player;
using UnityEngine;

namespace Gameplay.Visual.Board
{
    public class BoardSideVisual
    {
        public MandarinTileVisual MandarinTileVisual;
        public IReadOnlyList<CitizenTileVisual> CitizenTiles;
    }

    public class BoardMetadata
    {
        public float TileSize;
        public int NumCitizenTilesPerSide;
        public IReadOnlyList<Vector2> Polygon;
    }

    public class BoardVisual : MonoBehaviour
    {
        public IReadOnlyList<TileVisual> TileVisuals { get; private set; }
        public IReadOnlyList<BoardSideVisual> SideVisuals { get; private set;}
        public IReadOnlyList<PieceBench> PocketVisuals { get; private set;}
        public BoardMetadata Metadata { get; private set;}

        public void SetReferences(
            IReadOnlyList<BoardSideVisual> sides,
            IReadOnlyList<TileVisual> tiles, 
            IReadOnlyList<PieceBench> pocketVisuals, 
            BoardMetadata metadata)
        {
            SideVisuals = sides;
            TileVisuals = tiles;
            PocketVisuals = pocketVisuals;
            Metadata = metadata;
        }
    }
}