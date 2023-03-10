using Framework.Resolver;
using Gameplay.CoreGameplay;
using Gameplay.CoreGameplay.Controllers;
using Gameplay.Entities.Stage.StageSelector;
using Gameplay.GameInteract;
using Gameplay.GameState;
using Gameplay.Helpers;
using Gameplay.PlayTurn;
using Gameplay.Visual;
using Gameplay.Visual.Presenters;
using Gameplay.Visual.Views;
using UnityEngine;

namespace System
{
    [CreateAssetMenu]
    public class GameplayLauncher : BaseDependencyInversionScriptableObject
    {
        private Gameplay _gameplay;
        private PlayerInteract _interact;
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
            _interact = Resolver.Resolve<PlayerInteract>();
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
            
            _interact.Initialize();

            _gameplay = new Gameplay(_container, _interact, _boardStatePresenter, _coreGameplayController, _boardVisualView);
            _gameplay.GameOverEvent -= OnGameOver;
            _gameplay.GameOverEvent += OnGameOver;
        }

        private void ClearGame()
        {
            _gameplay.GameOverEvent -= OnGameOver;

            _interact.Cleanup();
            _boardVisualView.Cleanup();
            _gameplay.Cleanup();
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

        private void OnGameOver(Gameplay obj)
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