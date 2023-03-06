using Framework.Resolver;
using Gameplay.CoreGameplay;
using Gameplay.CoreGameplay.Controllers;
using Gameplay.Entities.Stage.StageSelector;
using Gameplay.GameInteract;
using Gameplay.GameState;
using Gameplay.Helpers;
using Gameplay.PlayTurn;
using Gameplay.Visual;
using UnityEngine;

namespace System
{
    /// <summary>
    /// Only use event when:
    /// the listeners are sequentially independent of each other, otherwise, their states would be changed
    /// on handle the event, any one that relies on a specific state of a listener might get into
    /// trouble. Is there any solution for this problem?
    /// </summary>
    [CreateAssetMenu]
    public class GameplayLauncher : BaseDependencyInversionScriptableObject
    {
        private Gameplay _gameplay;
        private GridLocator _gridLocator;
        private IPlayerInteract _interact;
        private IStageSelector _stageSelector;
        private IGameState _gameState;
        private GameStateController _gameStateController;
        private CoreGameplayVisualPresenter _coreGameplayVisualPresenter;
        private ICoreGameplayController _coreGameplayController;
        private CoreGameplayLauncher _coreGameplayLauncher;
        private readonly GameplayContainer _container = new();

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
            _gridLocator = Resolver.Resolve<GridLocator>();
            _coreGameplayLauncher = Resolver.Resolve<CoreGameplayLauncher>();
            _coreGameplayVisualPresenter = Resolver.Resolve<CoreGameplayVisualPresenter>();
            _coreGameplayController = Resolver.Resolve<ICoreGameplayController>();

            _gameState = Resolver.Resolve<IGameState>();
            _gameState.StateChangedEvent -= OnGameStateChanged;
            _gameState.StateChangedEvent += OnGameStateChanged;
        }

        protected override void OnTearDownDependencies()
        {
            base.OnTearDownDependencies();
            _gameState.StateChangedEvent -= OnGameStateChanged;
            _gameplay.GameOverEvent -= OnGameOver;
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

        private void OnGameOver(Gameplay obj)
        {
            _gameStateController.EndGame();
        }

        private void StartGame()
        {
            _container.PublicMatchData(_stageSelector.SelectedStage.Data.MatchData);
            _coreGameplayLauncher.Load();

            _coreGameplayController.RequestRefresh(_coreGameplayVisualPresenter);
            _coreGameplayController.RequestRefresh(Resolver.Resolve<BoardStatePresenter>());

            _container.PublicPlayTurnTeller(new PlayTurnDataGenerator(_coreGameplayVisualPresenter,
                Resolver.Resolve<BoardStatePresenter>().BoardStateViewData).Generate());
            _interact.Initialize();

            _gameplay = new Gameplay(_container.PlayTurnTeller, _interact,
                Resolver.Resolve<BoardStatePresenter>(), _coreGameplayController,
                _coreGameplayVisualPresenter);
            _gameplay.GameOverEvent -= OnGameOver;
            _gameplay.GameOverEvent += OnGameOver;
            _gameplay.Initialize(_coreGameplayVisualPresenter.BoardVisual);
            ConnectVisualPresenterEvents();
        }

        private void ClearGame()
        {
            _gameplay.GameOverEvent -= OnGameOver;
            _interact.Cleanup();
            _coreGameplayVisualPresenter.Cleanup();
            _gameplay.Cleanup();
            _coreGameplayLauncher.Unload();
            _container.Cleanup();
        }

        private void ConnectVisualPresenterEvents()
        {
            _coreGameplayVisualPresenter.VisualReadyEvent -= OnVisualPresenterReady;
            _coreGameplayVisualPresenter.VisualReadyEvent += OnVisualPresenterReady;
        }

        private void DisconnectVisualPresenterEvents()
        {
            _coreGameplayVisualPresenter.VisualReadyEvent -= OnVisualPresenterReady;
        }

        private void OnVisualPresenterReady(CoreGameplayVisualPresenter obj)
        {
            DisconnectVisualPresenterEvents();
            _gameplay.Start();
        }


//
// #if UNITY_EDITOR
//         [SerializeField] private int testIndex;
//         [SerializeField] private int testIndex2;
//         [SerializeField] private bool testDirection;
//         [SerializeField] private bool reset;
//
//         private int[] _state;
//         [ContextMenu("TestLogState")]
//         public void TestLogState()
//         {
//             if (reset || _state == null || _state.Length == 0)
//             {
//                 _state = new int[12];
//                 for (var i = 0; i < 12; i++)
//                 {
//                     _state[i] = i % 6 == 0 ? 10 : 5;
//                 }
//
//                 reset = false;
//             }
//
//             var steps = BoardStateCalculator.Calculate(_state, testIndex, testDirection);
//             var steps2 = BoardStateCalculator.Calculate(_state, testIndex2, testDirection);
//             var count = 0;
//             while (true)
//             {
//                 var a = steps.MoveNext();
//                 var b = steps2.MoveNext();
//                 if (!a && !b) break;
//
//                 var str = "";
//                 for (var i = 0; i < _state.Length; i++)
//                 {
//                     var s = steps.Current.State == 1 && i == steps.Current.TileIndex ? $"({steps.Current.Data})" : "";
//                     var s2 = steps2.Current.State == 1 && i == steps2.Current.TileIndex
//                         ? $"({steps2.Current.Data})"
//                         : "";
//                     var hit = a && steps.Current.State == 2 && i == steps.Current.TileIndex ||
//                               b && steps2.Current.State == 2 && i == steps2.Current.TileIndex;
//                     var eat = a && steps.Current.State == 3 && i == steps.Current.TileIndex ||
//                               b && steps2.Current.State == 3 && i == steps2.Current.TileIndex;
//                     str += $" {(eat ? "(" : "")}{(hit ? "X" : _state[i])}{(eat ? ")" : "")}{s}{s2} -";
//                 }
//
//                 var stepA = $"({steps.Current.State} {steps.Current.TileIndex})";
//                 var stepB = $"&({steps2.Current.State} {steps2.Current.TileIndex})";
//                 var eatenA = steps.Current.State == 3;
//                 var eatenB = steps2.Current.State == 3;
//                 Debug.Log(
//                     $"{count++} {(a ? stepA : "")}{(b ? stepB : "")}: {str} {(eatenA ? $">[{steps.Current.Data}]" : "")}{(eatenB ? $">[{steps2.Current.Data}]" : "")}");
//             }
//         }
// #endif
    }
}