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
            var piecesInTiles = _boardEntity.Sides.SelectMany(s => new[] {s.MandarinTile.PieceEntities.Count}.Concat(s.CitizenTiles.Select(c => c.PieceEntities.Count)));
            var piecesInPockets = _boardEntity.Sides.Select(s => s.Pocket.PieceEntities.Count);
            return new RefreshData {PiecesInTiles = piecesInTiles.ToArray(), PiecesInPockets = piecesInPockets.ToArray()};
        }
    }

    public interface IRefreshResultHandler
    {
        void HandleRefreshData(RefreshData refreshData);
    }

    public class RefreshData
    {
        public int[] PiecesInTiles;
        public int[] PiecesInPockets;
    }
}