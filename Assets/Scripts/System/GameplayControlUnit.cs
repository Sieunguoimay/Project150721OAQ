using System.Collections;
using System.Linq;
using Framework.Resolver;
using Gameplay;
using Gameplay.BambooStick;
using Gameplay.Board;
using Gameplay.Entities.Stage;
using Gameplay.Entities.Stage.StageSelector;
using Gameplay.GameInteract;
using Gameplay.Piece;
using UnityEngine;

namespace System
{
    /// <summary>
    /// Only use event when:
    /// the listeners are sequentially independent of each other, otherwise, their states would be changed
    /// on handle the event, any one that relies on a specific state of a listener might get into
    /// trouble. Is there any solution for this problem?
    /// </summary>
    public class GameplayControlUnit : MonoControlUnitBase<GameplayControlUnit>
    {
        private readonly Gameplay _gameplay = new();

        private BambooFamilyManager _bambooFamily;
        private PlayersManager _playersManager;
        private BoardManager _boardManager;
        private PieceManager _pieceManager;
        private GameInteractManager _interact;
        private IStageSelector _stageSelector;

        public event Action GameplayBeginEvent;
        public event Action GameplayEndEvent;

        protected override void OnInject(IResolver resolver)
        {
            _playersManager = resolver.Resolve<PlayersManager>();
            _boardManager = resolver.Resolve<BoardManager>();
            _pieceManager = resolver.Resolve<PieceManager>();
            _bambooFamily = resolver.Resolve<BambooFamilyManager>();
            _stageSelector = resolver.Resolve<IStageSelector>("stage_selector");
            _interact = resolver.Resolve<GameInteractManager>();
        }

        private void Update()
        {
            _gameplay.ActivityQueue.Update(Time.deltaTime);
        }

        public void StartGame()
        {
            StartGameCoroutine();
            GameplayBeginEvent?.Invoke();
        }

        private void StartGameCoroutine()
        {
            GenerateMatch(_stageSelector.SelectedStage, () => { _gameplay.StartNewMatch(); });
        }

        private void GenerateMatch(IStage stage, Action done)
        {
            _boardManager.CreateBoard(stage.Data.PlayerNum, stage.Data.TilesPerGroup);

            _playersManager.FillWithFakePlayers(stage.Data.PlayerNum);
            _playersManager.CreatePieceBench(_boardManager.Board);

            _gameplay.Setup(_playersManager.Players, _boardManager.Board, _interact);

            _pieceManager.SpawnPieces(stage.Data.PlayerNum, stage.Data.TilesPerGroup, stage.Data.NumCitizensInTile);
            _pieceManager.ReleasePieces(done, _boardManager.Board);

            _bambooFamily.BeginAnimSequence();
        }

        public void ClearGame()
        {
            _bambooFamily.ResetAll();
            _pieceManager.DeletePieces();
            _gameplay.ClearGame();
            _playersManager.DeletePlayers();
            _boardManager.DeleteBoard();
            GameplayEndEvent?.Invoke();
        }

#if UNITY_EDITOR
        [SerializeField] private int testIndex;
        [SerializeField] private int testIndex2;
        [SerializeField] private bool testDirection;

        private int[] _state;
        [ContextMenu("TestLogState")]
        public void TestLogState()
        {
            
            _state ??= _boardManager.Board.Tiles
                .Select(t => t.HeldPieces.Count + (t is IMandarinTile {Mandarin: { }} ? 10 : 0)).ToArray();

            var steps = BoardStateCalculator.Calculate(_state, testIndex, testDirection);
            var steps2 = BoardStateCalculator.Calculate(_state, testIndex2, testDirection);
            var count = 0;
            while (true)
            {
                var a = steps.MoveNext();
                var b = steps2.MoveNext();
                var str = "";
                for (var i = 0; i < _state.Length; i++)
                {
                    var s = steps.Current.state == 1 && i == steps.Current.tileIndex ? $"({steps.Current.data})" : "";
                    var s2 = steps2.Current.state == 1 && i == steps2.Current.tileIndex ? $"({steps2.Current.data})" : "";
                    str += $"{_state[i]}{s}{s2} - ";
                }

                Debug.Log($"{count++} ({steps.Current.state} {steps.Current.tileIndex}) ({steps2.Current.state} {steps2.Current.tileIndex}): {str}");
                if (!a && !b) break;
            }
        }
#endif
    }
}