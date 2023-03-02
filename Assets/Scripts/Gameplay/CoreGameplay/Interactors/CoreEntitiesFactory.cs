using System.Collections.Generic;
using Gameplay.CoreGameplay.Entities;
using Gameplay.CoreGameplay.Gateway;

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
            var pocket = new PocketEntity {PieceEntities = new List<PieceEntity>()};
            var mandarinTile = new TileEntity {TileType = TileType.MandarinTile};
            var citizenTiles = new TileEntity[boardData.TilesPerSide];

            PopulatePiecesIntoContainer(mandarinTile, 1, PieceType.Mandarin);

            for (var i = 0; i < citizenTiles.Length; i++)
            {
                citizenTiles[i] = new TileEntity {TileType = TileType.CitizenTile};
                PopulatePiecesIntoContainer(citizenTiles[i], boardData.PiecesPerTile, PieceType.Citizen);
            }

            return new BoardEntity.BoardSide
            {
                Pocket = pocket,
                CitizenTiles = citizenTiles,
                MandarinTile = mandarinTile
            };
        }

        private static void PopulatePiecesIntoContainer(PieceContainerEntity tile, int numPieces, PieceType pieceType)
        {
            tile.PieceEntities = new List<PieceEntity>();
            for (var i = 0; i < numPieces; i++)
            {
                tile.PieceEntities.Add(new PieceEntity {PieceType = pieceType});
            }
        }
    }
}