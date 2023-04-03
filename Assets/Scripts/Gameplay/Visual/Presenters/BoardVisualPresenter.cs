using System;
using Framework.DependencyInversion;
using Gameplay.CoreGameplay.Interactors;
using Gameplay.Visual.Views;

namespace Gameplay.Visual.Presenters
{
    public class BoardVisualPresenter : 
        SelfBindingDependencyInversionUnit, 
        IRefreshResultHandler
    {
        private BoardVisualView _boardVisualView;

        public event Action<IRefreshResultHandler> BoardStateChangedEvent;
        protected override void OnSetupDependencies()
        {
            base.OnSetupDependencies();
            _boardVisualView = Resolver.Resolve<BoardVisualView>();
        }

        public void HandleRefreshData(RefreshData refreshData)
        {
            ExtractBoardState(refreshData);
            BoardStateChangedEvent?.Invoke(this);
        }

        private void ExtractBoardState(RefreshData refreshData)
        {
            _boardVisualView.RefreshVisual(refreshData);
        }
    }
}