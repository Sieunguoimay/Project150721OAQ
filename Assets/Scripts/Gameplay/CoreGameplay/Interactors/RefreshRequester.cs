using System.Linq;
using Gameplay.CoreGameplay.Entities;

namespace Gameplay.CoreGameplay.Interactors
{
    public interface IRefreshRequester
    {
        void Refresh();
    }

    public class RefreshRequester : IRefreshRequester
    {
        private readonly BoardEntity _boardEntity;
        private readonly IRefreshResultHandler _resultHandler;

        public RefreshRequester(BoardEntity boardEntity, IRefreshResultHandler resultHandler)
        {
            _boardEntity = boardEntity;
            _resultHandler = resultHandler;
        }

        public void Refresh()
        {
            var refreshOutputData = CreateRefreshData();
            _resultHandler?.HandleRefreshData(refreshOutputData);
        }

        private RefreshData CreateRefreshData()
        {
            var piecesInTiles = _boardEntity.Sides
                .SelectMany(s => new[] {CreatePieceStatistics(s.MandarinTile)}
                    .Concat(s.CitizenTiles.Select(CreatePieceStatistics)));
            var piecesInPockets = _boardEntity.Sides.Select(s => CreatePieceStatistics(s.Pocket));
            return new RefreshData
            {
                PiecesInTiles = piecesInTiles.ToArray(),
                PiecesInPockets = piecesInPockets.ToArray()
            };
        }

        private static RefreshData.PieceStatistics CreatePieceStatistics(PieceContainerEntity tile)
        {
            return new()
            {
                CitizenPieces = tile.PieceEntities.Count(p => p.PieceType == PieceType.Citizen),
                MandarinPieces = tile.PieceEntities.Count(p => p.PieceType == PieceType.Mandarin)
            };
        }
    }

    public interface IRefreshResultHandler
    {
        void HandleRefreshData(RefreshData refreshData);
    }

    public class RefreshData
    {
        public PieceStatistics[] PiecesInTiles;
        public PieceStatistics[] PiecesInPockets;

        public class PieceStatistics
        {
            public int CitizenPieces;
            public int MandarinPieces;
        }
    }
}