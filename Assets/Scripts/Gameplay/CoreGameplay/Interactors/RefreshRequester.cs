using System.Linq;
using Gameplay.CoreGameplay.Entities;
using Gameplay.CoreGameplay.Gateway;

namespace Gameplay.CoreGameplay.Interactors
{
    public interface IRefreshRequester
    {
        void Refresh(IRefreshResultHandler resultHandler);
    }

    public class RefreshRequester : IRefreshRequester
    {
        private readonly BoardEntityAccess _boardEntityAccess;
        private readonly ICoreGameplayDataAccess _dataAccess;
        private IRefreshResultHandler _resultHandler;

        public RefreshRequester(BoardEntityAccess boardEntityAccess, ICoreGameplayDataAccess dataAccess)
        {
            _boardEntityAccess = boardEntityAccess;
            _dataAccess = dataAccess;
        }

        public void Refresh(IRefreshResultHandler resultHandler)
        {
            _resultHandler = resultHandler;
            var refreshOutputData = CreateRefreshData();
            _resultHandler?.HandleRefreshData(refreshOutputData);
        }

        private RefreshData CreateRefreshData()
        {
            // var piecesInTiles = _boardEntity.Sides.SelectMany(s
            //     => new[] {CreatePieceStatistics(s.MandarinTile)}.Concat(s.CitizenTiles.Select(CreatePieceStatistics)));
            // var piecesInPockets = _boardEntity.Sides.Select(s => CreatePieceStatistics(s.Pocket));
            // var piecesInSides = _boardEntity.Sides.Select(s => CreatePieceStatistics(s.CitizenTiles));

            var piecesInTiles = _boardEntityAccess.TileEntities.Select(CreatePieceStatistics);
            var piecesInPockets = _boardEntityAccess.Board.Pockets.Select(CreatePieceStatistics);

            var sides = _boardEntityAccess.Board.Pockets.Length;
            var tilesPerSide = _boardEntityAccess.Board.CitizenTiles.Length / sides;
            var piecesInSides = new RefreshData.PieceStatistics[sides];
            for (var i = 0; i < sides; i++)
            {
                piecesInSides[i] = CreatePieceStatistics(_boardEntityAccess.Board.CitizenTiles[(i * tilesPerSide)..((i + 1) * tilesPerSide)].Select(t => t as PieceContainerEntity).ToArray());
            }

            return new RefreshData
            {
                PiecesInTiles = piecesInTiles.ToArray(),
                PiecesInPockets = piecesInPockets.ToArray(),
                PiecesInSides = piecesInSides,
                BoardData = _dataAccess.GetBoardData()
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

        public BoardData BoardData;

        public class PieceStatistics
        {
            public int CitizenPiecesCount;
            public int MandarinPiecesCount;
        }
    }
}