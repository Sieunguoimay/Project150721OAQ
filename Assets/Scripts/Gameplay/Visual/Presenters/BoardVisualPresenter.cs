using System;
using Gameplay.CoreGameplay.Interactors;
using Gameplay.Visual.Views;

namespace Gameplay.Visual.Presenters
{
    public class BoardVisualPresenter : IRefreshResultHandler
    {
        public BoardVisualView BoardVisualView { get; set; }
        
        public event Action<IRefreshResultHandler> BoardStateChangedEvent;
        public void HandleRefreshData(RefreshData refreshData)
        {
            ExtractBoardState(refreshData);
            BoardStateChangedEvent?.Invoke(this);
        }

        private void ExtractBoardState(RefreshData refreshData)
        {
            BoardVisualView.RefreshVisual(refreshData);
        }
    }
}