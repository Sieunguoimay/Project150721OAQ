using System.Collections.Generic;
using Gameplay.CoreGameplay.Entities;

namespace Gameplay.CoreGameplay.Interactors
{
    public static class CoreEntitiesFactory
    {
        public static BoardEntity CreateBoardEntity(BoardData boardData)
        {
            return new()
            {
                Sides = CreateBoardSides(boardData)
            };
        }

        private static BoardEntity.BoardSide[] CreateBoardSides(BoardData boardData)
        {
            var sides = new BoardEntity.BoardSide[boardData.NumSides];

            for (var i = 0; i < boardData.NumSides; i++)
            {
                sides[i] = CreateBoardSide(boardData);
            }

            return sides;
        }

        private static BoardEntity.BoardSide CreateBoardSide(BoardData boardData)
        {
            var pocket = new PocketEntity();
            var mandarinTile = new TileEntity();
            var citizenTiles = new TileEntity[boardData.TilesPerSide];

            PopulatePiecesIntoContainer(pocket, 0);
            PopulatePiecesIntoContainer(mandarinTile, 1);

            for (var i = 0; i < citizenTiles.Length; i++)
            {
                citizenTiles[i] = new TileEntity();
                PopulatePiecesIntoContainer(citizenTiles[i], boardData.PiecesPerTile);
            }

            return new BoardEntity.BoardSide
            {
                Pocket = pocket,
                CitizenTiles = citizenTiles,
                MandarinTile = mandarinTile
            };
        }

        private static void PopulatePiecesIntoContainer(PieceContainerEntity tile, int numPieces)
        {
            tile.PieceEntities = new List<PieceEntity>();
            for (var i = 0; i < numPieces; i++)
            {
                tile.PieceEntities.Add(new PieceEntity());
            }
        }
    }
}