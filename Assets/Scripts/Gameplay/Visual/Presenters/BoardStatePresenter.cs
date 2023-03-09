using System;
using Gameplay.CoreGameplay.Interactors;

namespace Gameplay.Visual.Presenters
{
    public class BoardStatePresenter : IRefreshResultHandler
    {
        public BoardStateView BoardStateView { get; } = new();
        public event Action<BoardStatePresenter> BoardStateChangedEvent;

        public void HandleRefreshData(RefreshData refreshData)
        {
            ExtractBoardState(refreshData);
            BoardStateChangedEvent?.Invoke(this);
        }

        private void ExtractBoardState(RefreshData refreshData)
        {
            BoardStateView.SetRefreshData(refreshData);
        }
    }

    public class BoardStateView
    {
        public RefreshData RefreshData { get; private set; }

        public void SetRefreshData(RefreshData refreshData)
        {
            RefreshData = refreshData;
        }

        public int NumSides => RefreshData.PiecesInPockets.Length;
        public bool AnyMandarinTileHasPieces => CheckAnyMandarinTileHasPieces();

        private bool CheckAnyMandarinTileHasPieces()
        {
            var numSides = RefreshData.PiecesInPockets.Length;
            for (var i = 0; i < numSides; i++)
            {
                if (CheckAnyPieceInMandarinTile(i)) return true;
            }

            return false;
        }

        public bool CheckAnyCitizenTileOnSideHasPieces(int sideIndex)
        {
            return RefreshData.PiecesInSides[sideIndex].CitizenPiecesCount > 0;
        }

        public bool CheckBenchOnSideHasPieces(int sideIndex)
        {
            return RefreshData.PiecesInPockets[sideIndex].CitizenPiecesCount > 0;
        }

        private bool CheckAnyPieceInMandarinTile(int sideIndex)
        {
            var numSides = RefreshData.PiecesInPockets.Length;
            var numTilesPerSide = RefreshData.PiecesInTiles.Length / numSides;
            var tile = RefreshData.PiecesInTiles[sideIndex * numTilesPerSide];
            return tile.MandarinPiecesCount > 0 || tile.CitizenPiecesCount > 0;
        }
    }
}