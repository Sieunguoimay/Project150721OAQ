using Framework.DependencyInversion;
using Framework.Resolver;
using Gameplay.CoreGameplay.Controllers;
using Gameplay.Visual.Presenters;
using Gameplay.Visual.Views;

namespace System
{
    public class GameplayLauncher : DependencyInversionScriptableObjectNode
    {
        private GameplayEventsHandler _gameplayEventsHandler;
        private ICoreGameplayController _coreGameplayController;
        private BoardStatePresenter _boardStatePresenter;
        private BoardVisualPresenter _boardVisualPresenter;
        private BoardVisualView _boardVisualView;
        private PiecesMovingRunner _movingRunner;
        private PiecesMovingRunner _movingRunner2;
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
            _boardVisualView = Resolver.Resolve<BoardVisualView>();
            _movingRunner = Resolver.Resolve<MultiThreadPiecesMovingRunner>();
            _movingRunner2 = Resolver.Resolve<SingleThreadPiecesMovingRunner>();
        }
        public void StartGame()
        {
            _coreGameplayController.SetupNewGame();
            _coreGameplayController.RequestRefresh(_boardVisualPresenter);
            _coreGameplayController.RequestRefresh(_boardStatePresenter);
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
    }
}