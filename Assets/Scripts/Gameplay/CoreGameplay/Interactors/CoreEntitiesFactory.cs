using System.Collections.Generic;
using Gameplay.CoreGameplay.Entities;
using Gameplay.CoreGameplay.Gateway;

namespace Gameplay.CoreGameplay.Interactors
{
    public static class CoreEntitiesFactory
    {
        public static BoardEntity CreateBoardEntity(BoardData boardData)
        {
            var boardEntity = new BoardEntity
            {
                // Sides = CreateBoardSides(boardData)
                MandarinTiles = CreateTiles(boardData.NumSides),
                CitizenTiles = CreateTiles(boardData.PiecesPerTile * boardData.NumSides),
                Pockets = CreatePockets(boardData.NumSides)
            };
            PopulatePiecesIntoContainers(boardEntity.MandarinTiles, 1, PieceType.Mandarin);
            PopulatePiecesIntoContainers(boardEntity.CitizenTiles, boardData.PiecesPerTile, PieceType.Citizen);
            return boardEntity;
        }

        // private static BoardEntity.BoardSide[] CreateBoardSides(BoardData boardData)
        // {
        //     var sides = new BoardEntity.BoardSide[boardData.NumSides];
        //
        //     for (var i = 0; i < boardData.NumSides; i++)
        //     {
        //         sides[i] = CreateBoardSide(boardData);
        //     }
        //
        //     return sides;
        // }
        //
        // private static BoardEntity.BoardSide CreateBoardSide(BoardData boardData)
        // {
        //     var pocket = new PocketEntity {PieceEntities = new List<PieceEntity>()};
        //     var mandarinTile = new TileEntity {TileType = TileType.MandarinTile};
        //     var citizenTiles = new TileEntity[boardData.TilesPerSide];
        //
        //     PopulatePiecesIntoContainer(mandarinTile, 1, PieceType.Mandarin);
        //
        //     for (var i = 0; i < citizenTiles.Length; i++)
        //     {
        //         citizenTiles[i] = new TileEntity {TileType = TileType.CitizenTile};
        //         PopulatePiecesIntoContainer(citizenTiles[i], boardData.PiecesPerTile, PieceType.Citizen);
        //     }
        //
        //     return new BoardEntity.BoardSide
        //     {
        //         Pocket = pocket,
        //         CitizenTiles = citizenTiles,
        //         MandarinTile = mandarinTile
        //     };
        // }
        private static void PopulatePiecesIntoContainers(IEnumerable<PieceContainerEntity> tileEntities, int piecesPerTile,
            PieceType pieceType)
        {
            foreach (var tileEntity in tileEntities)
            {
                PopulatePiecesIntoContainer(tileEntity, piecesPerTile, pieceType);
            }
        }

        private static void PopulatePiecesIntoContainer(PieceContainerEntity tile, int numPieces, PieceType pieceType)
        {
            tile.PieceEntities = new List<PieceEntity>();
            for (var i = 0; i < numPieces; i++)
            {
                tile.PieceEntities.Add(new PieceEntity {PieceType = pieceType});
            }
        }

        private static PocketEntity[] CreatePockets(int num)
        {
            var pockets = new PocketEntity[num];
            for (var i = 0; i < num; i++)
            {
                var pocket = new PocketEntity {PieceEntities = new List<PieceEntity>()};
                pockets[i] = pocket;
            }

            return pockets;
        }

        private static TileEntity[] CreateTiles(int num)
        {
            var tiles = new TileEntity[num];
            for (var i = 0; i < num; i++)
            {
                var tile = new TileEntity {PieceEntities = new List<PieceEntity>()};
                tiles[i] = tile;
            }

            return tiles;
        }
    }
}