using Framework.Resolver;
using Gameplay.CoreGameplay.Controllers;
using Gameplay.Entities.Stage.StageSelector;
using Gameplay.GameInteract;
using Gameplay.GameState;
using Gameplay.PlayTurn;
using Gameplay.Visual.Presenters;
using Gameplay.Visual.Views;
using UnityEngine;

namespace System
{
    [CreateAssetMenu]
    public class GameplayLauncher : BaseDependencyInversionScriptableObject
    {
        private GameplayViewActionCompleteHandler _gameplayViewActionCompleteHandler;
        // private PlayerInteract _interact;
        private IStageSelector _stageSelector;
        private IGameState _gameState;
        private GameStateController _gameStateController;
        private ICoreGameplayController _coreGameplayController;
        private readonly GameplayContainer _container = new();
        private BoardStatePresenter _boardStatePresenter;
        private BoardVisualPresenter _boardVisualPresenter;
        private BoardVisualView _boardVisualView;
        private PiecesMovingRunner _piecesMovingRunner;
        private readonly PlayTurnDataGenerator _playTurnDataGenerator = new();

        protected override void OnBind(IBinder binder)
        {
            base.OnBind(binder);
            binder.Bind<IGameplayContainer>(_container);
        }

        protected override void OnUnbind(IBinder binder)
        {
            base.OnUnbind(binder);
            binder.Unbind<IGameplayContainer>();
        }

        protected override void OnSetupDependencies()
        {
            base.OnSetupDependencies();

            _stageSelector = Resolver.Resolve<IStageSelector>("stage_selector");
            // _interact = Resolver.Resolve<PlayerInteract>();
            _gameStateController = Resolver.Resolve<GameStateController>();
            _coreGameplayController = Resolver.Resolve<ICoreGameplayController>();
            _boardStatePresenter = Resolver.Resolve<BoardStatePresenter>();
            _boardVisualPresenter = Resolver.Resolve<BoardVisualPresenter>();
            _boardVisualView = Resolver.Resolve<BoardVisualView>();
            _piecesMovingRunner = Resolver.Resolve<PiecesMovingRunner>();
            _boardVisualPresenter.BoardVisualView = _boardVisualView;
            _gameState = Resolver.Resolve<IGameState>();
            _gameState.StateChangedEvent -= OnGameStateChanged;
            _gameState.StateChangedEvent += OnGameStateChanged;
        }

        protected override void OnTearDownDependencies()
        {
            base.OnTearDownDependencies();
            _gameState.StateChangedEvent -= OnGameStateChanged;
        }

        private void StartGame()
        {
            PublicCommonStuffs();

            _coreGameplayController.Install();

            RefreshVisual();

            _playTurnDataGenerator.Generate(_boardStatePresenter.BoardStateView.NumSides, _boardVisualView.BoardVisual);
            
            // _interact.Initialize();

            _gameplayViewActionCompleteHandler = new GameplayViewActionCompleteHandler(_container, _boardStatePresenter, _coreGameplayController, _boardVisualView);
        }

        private void ClearGame()
        {
            // _interact.Cleanup();
            _boardVisualView.Cleanup();
            _gameplayViewActionCompleteHandler.Cleanup();
            _coreGameplayController.Uninstall();
            _container.Cleanup();
        }

        private void PublicCommonStuffs()
        {
            _container.PublicMatchData(_stageSelector.SelectedStage.Data.MatchData);
            _container.PublicMovingRunner(_piecesMovingRunner);
            _container.PublicPlayTurnTeller(_playTurnDataGenerator.PlayTurnTeller);
        }

        private void RefreshVisual()
        {
            _coreGameplayController.RequestRefresh(_boardVisualPresenter);
            _coreGameplayController.RequestRefresh(_boardStatePresenter);
        }

        private void OnGameOver(GameplayViewActionCompleteHandler obj)
        {
            _gameStateController.EndGame();
        }

        private void OnGameStateChanged(GameState gameState)
        {
            if (gameState.CurrentState == GameState.State.Playing)
            {
                StartGame();
            }
            else if (gameState.CurrentState == GameState.State.InMenu)
            {
                ClearGame();
            }
        }
    }
}