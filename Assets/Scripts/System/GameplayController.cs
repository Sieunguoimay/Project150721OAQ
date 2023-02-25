using System.Collections;
using System.Linq;
using Framework.Resolver;
using Gameplay;
using Gameplay.BambooStick;
using Gameplay.Board;
using Gameplay.Entities.Stage;
using Gameplay.Entities.Stage.StageSelector;
using Gameplay.GameInteract;
using Gameplay.GameState;
using Gameplay.Piece;
using Gameplay.Player;
using UnityEngine;

namespace System
{
    public interface IGameplayController
    {
        void StartGame();
        void ClearGame();
        event Action<IGameplayController> StartedGameEvent;
        event Action<IGameplayController> ClearedGameEvent;
        event Action<IGameplayController> GameEndedEvent;
    }

    /// <summary>
    /// Only use event when:
    /// the listeners are sequentially independent of each other, otherwise, their states would be changed
    /// on handle the event, any one that relies on a specific state of a listener might get into
    /// trouble. Is there any solution for this problem?
    /// </summary>
    public class GameplayController : BaseGenericDependencyInversionUnit<IGameplayController>, IGameplayController
    {
        private IGameplay _gameplay;

        private BambooFamilyManager _bambooFamily;
        private IPlayerManager _playersManager;
        private BoardManager _boardManager;
        private PieceManager _pieceManager;
        private IPlayerInteract _interact;
        private IStageSelector _stageSelector;
        private IGameState _gameState;

        public event Action<IGameplayController> StartedGameEvent;
        public event Action<IGameplayController> ClearedGameEvent;
        public event Action<IGameplayController> GameEndedEvent;

        protected override void OnSetupDependencies()
        {
            base.OnSetupDependencies();

            _playersManager = Resolver.Resolve<PlayerController>();
            _boardManager = Resolver.Resolve<BoardManager>();
            _pieceManager = Resolver.Resolve<PieceManager>();
            _bambooFamily = Resolver.Resolve<BambooFamilyManager>();
            _stageSelector = Resolver.Resolve<IStageSelector>("stage_selector");
            _interact = Resolver.Resolve<PlayerInteract>();

            _gameplay = new Gameplay(_playersManager, _boardManager, _interact);
            _gameplay.GameOverEvent -= OnGameOver;
            _gameplay.GameOverEvent += OnGameOver;
            _gameState = Resolver.Resolve<IGameState>();

            _gameState.StateChangedEvent -= OnGameStateChanged;
            _gameState.StateChangedEvent += OnGameStateChanged;
        }

        protected override void OnTearDownDependencies()
        {
            base.OnTearDownDependencies();
            _gameState.StateChangedEvent -= OnGameStateChanged;
            _gameState.StateChangedEvent += OnGameStateChanged;
            _gameplay.GameOverEvent -= OnGameOver;
        }

        private void OnGameOver(IGameplay obj)
        {
            GameEndedEvent?.Invoke(this);
        }

        private void OnGameStateChanged(GameState gameState)
        {
            if (gameState.CurrentSate == GameState.State.Playing)
            {
                StartGame();
            }
            else if (gameState.CurrentSate == GameState.State.InMenu)
            {
                ClearGame();
            }
        }

        public void StartGame()
        {
            StartGameCoroutine();
            StartedGameEvent?.Invoke(this);
        }

        private void StartGameCoroutine()
        {
            GenerateMatch(_stageSelector.SelectedStage);
        }

        private void GenerateMatch(IStage stage)
        {
            _boardManager.CreateBoard(stage.Data.PlayerNum, stage.Data.TilesPerGroup);

            _playersManager.FillUpWithFakePlayers(stage.Data.PlayerNum);
            _playersManager.CreatePieceBench(_boardManager.Board);

            _pieceManager.SpawnPieces(stage.Data.PlayerNum, stage.Data.TilesPerGroup, stage.Data.NumCitizensInTile);
            _pieceManager.ReleasePieces(OnAllPiecesInPlace, _boardManager.Board);

            _bambooFamily.BeginAnimSequence();
            _interact.Initialize();
            _gameplay.Initialize();
        }

        private void OnAllPiecesInPlace()
        {
            _gameplay.Start();
        }

        public void ClearGame()
        {
            _interact.Cleanup();
            _bambooFamily.ResetAll();
            _pieceManager.DeletePieces();
            _gameplay.Cleanup();
            _playersManager.DeletePlayers();
            _boardManager.DeleteBoard();
            ClearedGameEvent?.Invoke(this);
        }

#if UNITY_EDITOR
        [SerializeField] private int testIndex;
        [SerializeField] private int testIndex2;
        [SerializeField] private bool testDirection;
        [SerializeField] private bool reset;

        private int[] _state;
        [ContextMenu("TestLogState")]
        public void TestLogState()
        {
            if (reset || _state == null || _state.Length == 0)
            {
                _state = new int[12];
                for (var i = 0; i < 12; i++)
                {
                    _state[i] = i % 6 == 0 ? 10 : 5;
                }

                reset = false;
            }

            var steps = BoardStateCalculator.Calculate(_state, testIndex, testDirection);
            var steps2 = BoardStateCalculator.Calculate(_state, testIndex2, testDirection);
            var count = 0;
            while (true)
            {
                var a = steps.MoveNext();
                var b = steps2.MoveNext();
                if (!a && !b) break;

                var str = "";
                for (var i = 0; i < _state.Length; i++)
                {
                    var s = steps.Current.State == 1 && i == steps.Current.TileIndex ? $"({steps.Current.Data})" : "";
                    var s2 = steps2.Current.State == 1 && i == steps2.Current.TileIndex
                        ? $"({steps2.Current.Data})"
                        : "";
                    var hit = a && steps.Current.State == 2 && i == steps.Current.TileIndex ||
                              b && steps2.Current.State == 2 && i == steps2.Current.TileIndex;
                    var eat = a && steps.Current.State == 3 && i == steps.Current.TileIndex ||
                              b && steps2.Current.State == 3 && i == steps2.Current.TileIndex;
                    str += $" {(eat ? "(" : "")}{(hit ? "X" : _state[i])}{(eat ? ")" : "")}{s}{s2} -";
                }

                var stepA = $"({steps.Current.State} {steps.Current.TileIndex})";
                var stepB = $"&({steps2.Current.State} {steps2.Current.TileIndex})";
                var eatenA = steps.Current.State == 3;
                var eatenB = steps2.Current.State == 3;
                Debug.Log(
                    $"{count++} {(a ? stepA : "")}{(b ? stepB : "")}: {str} {(eatenA ? $">[{steps.Current.Data}]" : "")}{(eatenB ? $">[{steps2.Current.Data}]" : "")}");
            }
        }
#endif
    }
}