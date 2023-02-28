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
            private readonly BoardEntity _board;

            public InnerPiecesInteractor(BoardEntity board)
            {
                _board = board;
                TileEntities = CreateBoardTilesEnumerable(_board).ToArray();
            }

            public TileEntity[] TileEntities { get; }

            public bool InnerMovePieceToNewTile(int currentTileIndex, int targetTileIndex)
            {
                var currentTile = GetTileAtIndex(currentTileIndex);
                var targetTile = GetTileAtIndex(targetTileIndex);

                return MoveSinglePieceFromContainerToContainer(currentTile, targetTile);
            }

            public bool InnerMovePiecesToPocket(int currentTileIndex, int targetPocketIndex)
            {
                var current = GetTileAtIndex(currentTileIndex);
                var target = GetPocketAtIndex(targetPocketIndex);

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


            private TileEntity GetTileAtIndex(int index)
            {
                return TileEntities[index];
            }

            private static IEnumerable<TileEntity> CreateBoardTilesEnumerable(BoardEntity boardEntity)
            {
                foreach (var boardSide in boardEntity.Sides)
                {
                    yield return boardSide.MandarinTile;
                    foreach (var t in boardSide.CitizenTiles)
                    {
                        yield return t;
                    }
                }
            }

            private PocketEntity GetPocketAtIndex(int index)
            {
                return _board.Sides[index].Pocket;
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