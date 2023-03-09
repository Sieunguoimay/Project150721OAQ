using System.Collections.Generic;
using System.Linq;
using Gameplay.CoreGameplay.Entities;

namespace Gameplay.CoreGameplay.Interactors
{
    public interface IPiecesInteractor
    {
        void MovePieceToNewTile(PieceInteractData.PieceMoveData moveData);
        void MovePiecesToPocket(PieceInteractData.PieceMoveToPocketData moveData);
    }

    public class PiecesInteractor : IPiecesInteractor
    {
        private readonly IPieceInteractResultHandler _resultHandler;
        private readonly InnerPiecesInteractor _innerPiecesInteractor;

        public PiecesInteractor(InnerPiecesInteractor innerPiecesInteractor, IPieceInteractResultHandler resultHandler)
        {
            _innerPiecesInteractor = innerPiecesInteractor;
            _resultHandler = resultHandler;
        }

        public void MovePieceToNewTile(PieceInteractData.PieceMoveData moveData)
        {
            var success = _innerPiecesInteractor.InnerMovePieceToNewTile(moveData.CurrentTileIndex, moveData.TargetTileIndex);
            var result = CreateResult(success);

            _resultHandler.OnMovePieceToNewTileDone(result);
        }

        public void MovePiecesToPocket(PieceInteractData.PieceMoveToPocketData moveData)
        {
            var success = _innerPiecesInteractor.InnerMovePiecesToPocket(moveData.CurrentTileIndex, moveData.TargetPocketIndex);
            var result = CreateResult(success);
            _resultHandler.OnMoveAllPiecesToPocketDone(result);
        }

        private static PieceInteractResultData CreateResult(bool success)
        {
            return new()
            {
                Success = success
            };
        }

        public class InnerPiecesInteractor
        {
            private readonly BoardEntityAccess _boardAccess;

            public InnerPiecesInteractor(BoardEntityAccess boardAccess)
            {
                _boardAccess = boardAccess;
            }
            public bool InnerMovePieceToNewTile(int currentTileIndex, int targetTileIndex)
            {
                var currentTile = _boardAccess.GetTileAtIndex(currentTileIndex);
                var targetTile = _boardAccess.GetTileAtIndex(targetTileIndex);

                return MoveSinglePieceFromContainerToContainer(currentTile, targetTile);
            }

            public bool InnerMovePiecesToPocket(int currentTileIndex, int targetPocketIndex)
            {
                var current = _boardAccess.GetTileAtIndex(currentTileIndex);
                var target = _boardAccess.GetPocketAtIndex(targetPocketIndex);

                return MoveAllPiecesFromContainerToContainer(current, target);
            }

            public static bool MoveAllPiecesFromContainerToContainer(PieceContainerEntity current, PieceContainerEntity target)
            {
                if (current.PieceEntities.Count <= 0) return false;
                target.PieceEntities.AddRange(current.PieceEntities);
                current.PieceEntities.Clear();
                return true;
            }

            public static bool MoveSinglePieceFromContainerToContainer(PieceContainerEntity current, PieceContainerEntity target)
            {
                var lastPiece = current.PieceEntities[^1];
                var success = current.PieceEntities.Remove(lastPiece);
                if (success)
                {
                    target.PieceEntities.Add(lastPiece);
                }

                return success;
            }

        }
    }

    public class BoardEntityAccess
    {
        public BoardEntityAccess(BoardEntity board)
        {
            Board = board;
            TileEntities = CreateBoardTilesEnumerable(Board).ToArray();
        }

        public TileEntity[] TileEntities { get; }

        public BoardEntity Board { get; }

        public TileEntity GetTileAtIndex(int index)
        {
            return TileEntities[index];
        }
        
        public PocketEntity GetPocketAtIndex(int index)
        {
            return Board.Pockets[index];
        }
        
        private static IEnumerable<TileEntity> CreateBoardTilesEnumerable(BoardEntity boardEntity)
        {
            var citizenTiles = boardEntity.CitizenTiles.Length / boardEntity.MandarinTiles.Length;
            for (var i = 0; i < boardEntity.MandarinTiles.Length; i++)
            {
                yield return boardEntity.MandarinTiles[i];
                for (var j = 0; j < citizenTiles; j++)
                {
                    yield return boardEntity.CitizenTiles[i * citizenTiles + j];
                }
            }
        }

    }

    public class PieceInteractData
    {
        public class PieceMoveToPocketData
        {
            public int CurrentTileIndex;
            public int TargetPocketIndex;
        }

        public class PieceMoveData
        {
            public int CurrentTileIndex;
            public int TargetTileIndex;
        }
    }

    public interface IPieceInteractResultHandler
    {
        void OnMovePieceToNewTileDone(PieceInteractResultData resultData);
        void OnMoveAllPiecesToPocketDone(PieceInteractResultData resultData);
    }

    public class PieceInteractResultData
    {
        public bool Success;
    }
}