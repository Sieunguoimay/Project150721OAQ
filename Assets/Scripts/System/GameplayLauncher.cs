using Framework.DependencyInversion;
using Framework.Resolver;
using Gameplay.CoreGameplay.Controllers;
using Gameplay.CoreGameplay.Interactors;
using Gameplay.Visual.Presenters;
using Gameplay.Visual.Views;
using System.Collections.Generic;

namespace System
{
    public class GameplayLauncher : ScriptableEntity, IRefreshResultHandler
    {
        private GameplayEventsHandler _gameplayEventsHandler;
        private ICoreGameplayController _coreGameplayController;
        private BoardVisualGenerator _boardVisualView;
        private PiecesMovingRunner _movingRunner;
        private PiecesMovingRunner _movingRunner2;

        private IRefreshResultHandler _boardStatePresenter;
        private IRefreshResultHandler _boardVisualPresenter;
        private IEnumerable<IRefreshResultHandler> RefreshResultHandlers
        {
            get
            {
                yield return _boardStatePresenter;
                yield return _boardVisualPresenter;
            }
        }
        protected override void OnBind(IBinder binder)
        {
            _gameplayEventsHandler = new GameplayEventsHandler();
            AddChildDependencyInversionUnit(_gameplayEventsHandler);
            AddChildDependencyInversionUnit(new GameStateEventsHandler());
            base.OnBind(binder);
        }

        protected override void OnSetupDependencies()
        {
            base.OnSetupDependencies();

            _coreGameplayController = Resolver.Resolve<ICoreGameplayController>();

            _boardStatePresenter = Resolver.Resolve<BoardStatePresenter>();
            _boardVisualPresenter = Resolver.Resolve<BoardVisualPresenter>();

            _boardVisualView = Resolver.Resolve<BoardVisualGenerator>();
            _movingRunner = Resolver.Resolve<MultiThreadPiecesMovingRunner>();
            _movingRunner2 = Resolver.Resolve<SingleThreadPiecesMovingRunner>();
        }

        public void StartGame()
        {
            _coreGameplayController.SetupNewGame();
            _coreGameplayController.RequestRefresh(this);
            _gameplayEventsHandler.SetupForNewGame();
        }

        public void ClearGame()
        {
            _boardVisualView.Cleanup();
            _gameplayEventsHandler.Cleanup();
            _coreGameplayController.TearDownGame();
            _movingRunner.ResetMovingSteps();
            _movingRunner2.ResetMovingSteps();
        }

        public void HandleRefreshData(RefreshData refreshData)
        {
            foreach (var item in RefreshResultHandlers)
            {
                item.HandleRefreshData(refreshData);
            }
        }
    }
}