using System.Linq;
using Gameplay.CoreGameplay.Entities;

namespace Gameplay.CoreGameplay.Interactors
{
    public interface IRefreshRequester
    {
        void Refresh(IRefreshResultHandler resultHandler);
    }

    public class RefreshRequester : IRefreshRequester
    {
        private readonly BoardEntity _boardEntity;
        private IRefreshResultHandler _resultHandler;

        public RefreshRequester(BoardEntity boardEntity)
        {
            _boardEntity = boardEntity;
        }

        public void Refresh(IRefreshResultHandler resultHandler)
        {
            _resultHandler = resultHandler;
            var refreshOutputData = CreateRefreshData();
            _resultHandler?.HandleRefreshData(refreshOutputData);
        }

        private RefreshData CreateRefreshData()
        {
            var piecesInTiles = _boardEntity.Sides
                .SelectMany(s => new[] {CreatePieceStatistics(s.MandarinTile)}
                    .Concat(s.CitizenTiles.Select(CreatePieceStatistics)));
            var piecesInPockets = _boardEntity.Sides.Select(s => CreatePieceStatistics(s.Pocket));
            var piecesInSides = _boardEntity.Sides.Select(s => CreatePieceStatistics(s.CitizenTiles));
            return new RefreshData
            {
                PiecesInTiles = piecesInTiles.ToArray(),
                PiecesInPockets = piecesInPockets.ToArray(),
                PiecesInSides = piecesInSides.ToArray()
            };
        }

        private static RefreshData.PieceStatistics CreatePieceStatistics(PieceContainerEntity tile)
        {
            return new()
            {
                CitizenPiecesCount = tile.PieceEntities.Count(p => p.PieceType == PieceType.Citizen),
                MandarinPiecesCount = tile.PieceEntities.Count(p => p.PieceType == PieceType.Mandarin)
            };
        }

        private static RefreshData.PieceStatistics CreatePieceStatistics(PieceContainerEntity[] tiles)
        {
            return new()
            {
                CitizenPiecesCount = tiles.Sum(t => t.PieceEntities.Count(p => p.PieceType == PieceType.Citizen)),
                MandarinPiecesCount = tiles.Sum(t => t.PieceEntities.Count(p => p.PieceType == PieceType.Mandarin))
            };
        }
    }

    public interface IRefreshResultHandler
    {
        void HandleRefreshData(RefreshData refreshData);
    }

    public class RefreshData
    {
        public PieceStatistics[] PiecesInSides;
        public PieceStatistics[] PiecesInTiles;
        public PieceStatistics[] PiecesInPockets;

        public class PieceStatistics
        {
            public int CitizenPiecesCount;
            public int MandarinPiecesCount;
        }
    }
}