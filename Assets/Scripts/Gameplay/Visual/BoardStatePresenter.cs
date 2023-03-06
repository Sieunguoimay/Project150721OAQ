using System;
using Gameplay.CoreGameplay.Interactors;

namespace Gameplay.Visual
{
    public class BoardStatePresenter : IRefreshResultHandler
    {
        public BoardStateView BoardStateViewData { get; } = new();
        public event Action<BoardStatePresenter> BoardStateChangedEvent;

        public void HandleRefreshData(RefreshData refreshData)
        {
            ExtractBoardState(refreshData);
            BoardStateChangedEvent?.Invoke(this);
        }

        private void ExtractBoardState(RefreshData refreshData)
        {
            BoardStateViewData.SetRefreshData(refreshData);
        }
    }

    public class BoardStateView
    {
        public RefreshData RefreshData { get; private set; }

        public void SetRefreshData(RefreshData refreshData)
        {
            RefreshData = refreshData;
        }

        public int NumSides => RefreshData.PiecesInSides.Length;
        public bool AnyMandarinTileHasPieces => CheckAnyMandarinTileHasPieces();

        private bool CheckAnyMandarinTileHasPieces()
        {
            var numSides = RefreshData.PiecesInPockets.Length;
            var numTilesPerSide = RefreshData.PiecesInTiles.Length / numSides;
            for (var i = 0; i < numSides; i++)
            {
                var tile = RefreshData.PiecesInTiles[i * numTilesPerSide];
                if (tile.MandarinPiecesCount > 0 || tile.CitizenPiecesCount > 0)
                {
                    return true;
                }
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
    }
}