using System.Linq;
using Common.Misc;
using Framework.Entities;
using Framework.Resolver;
using Framework.Services;
using Gameplay;
using Gameplay.BambooStick;
using Gameplay.Board;
using Gameplay.Entities.Stage;
using Gameplay.Entities.Stage.StageSelector;
using Gameplay.GameInteract;
using Gameplay.Piece;
using SNM;
using UnityEngine;

namespace System
{
    public class GameplayControlUnit : MonoControlUnitBase<GameplayControlUnit>
    {
        private readonly Gameplay _gameplay = new();

        private BambooFamilyManager _bambooFamily;
        private PlayersManager _playersManager;
        private BoardManager _boardManager;
        private PieceManager _pieceManager;

        private IResolver _resolver;
        private IStageSelector _stageSelector;

        private IGameplayUnit[] _gameplayUnits;

        protected override void OnInject(IResolver resolver)
        {
            _resolver = resolver;

            _playersManager = resolver.Resolve<PlayersManager>();
            _boardManager = resolver.Resolve<BoardManager>();
            _pieceManager = resolver.Resolve<PieceManager>();
            _bambooFamily = resolver.Resolve<BambooFamilyManager>();
            _stageSelector = resolver.Resolve<IStageSelector>("stage_selector");
            // RayPointer.Instance.SetCamera(resolver.Resolve<CameraManager>().Camera);

            // var matchProcessor = _resolver.Resolve<ICurrencyProcessor>(matchProcessorId);
            // _resolver.Resolve<IMessageService>().Register<IMessage<ICurrencyProcessor>, ICurrencyProcessor>(MatchProcessorSuccess, matchProcessor);
            _gameplayUnits = new IGameplayUnit[] {_playersManager, _bambooFamily};
        }

        // private void OnEnable()
        // {
        //     _matchChooser.OnMatchOptionChanged += OnMatchChooserResult;
        // }
        //
        // private void OnDisable()
        // {
        //     _matchChooser.OnMatchOptionChanged -= OnMatchChooserResult;
        // }

        // private void OnCleanup()
        // {
        //     _gameplay.TearDown();
        // }

        private void Update()
        {
            _gameplay.ActivityQueue.Update(Time.deltaTime);
        }

        // private void OnMatchChooserResult()
        // {
        //     // _resolver.Resolve<ICurrencyProcessor>(matchProcessorId).Process();
        // }

        // private void MatchProcessorSuccess(IMessage<ICurrencyProcessor> message)
        // {
        //     var playerNum = _matchChooser.MatchOption.PlayerNum;
        //     var tilesPerGroup = _matchChooser.MatchOption.TilesPerGroup;
        //     GenerateMatch(playerNum, tilesPerGroup);
        //     StartGame();
        // }

        public void GenerateMatch(IStage stage)
        {
            _boardManager.CreateBoard(stage.Data.PlayerNum, stage.Data.TilesPerGroup);

            _playersManager.FillWithFakePlayers(stage.Data.PlayerNum);
            _playersManager.CreatePieceBench(_boardManager.Board);

            _gameplay.Setup(_playersManager, _boardManager.Board, _pieceManager, _resolver.Resolve<GameInteractManager>());

            _pieceManager.SpawnPieces(stage.Data.PlayerNum, stage.Data.TilesPerGroup, stage.Data.NumCitizensInTile);

            _bambooFamily.BeginAnimSequence();
        }

        public void StartGame()
        {
            GenerateMatch(_stageSelector.SelectedStage);
            _gameplay.StartNewMatch();
            
            foreach (var gameplayUnit in _gameplayUnits)
            {
                gameplayUnit.OnGameplayStart();
            }
        }

        public void ClearGame()
        {
            foreach (var gameplayUnit in _gameplayUnits)
            {
                gameplayUnit.OnGameplayStop();
            }

            // _pieceManager.ClearAll();
            // _boardManager.ClearAll();
            // _playersManager.ClearAll();
            // _gameplay.ClearGame();
        }

        //
        // public void ReplayMatch()
        // {
        //     _gameplay.ResetGame();
        // }
        //
        // public void ResetGame()
        // {
        //     _gameplay.ResetGame();
        //     _gameplay.TearDown();
        // }
        public interface IGameplayUnit
        {
            void OnGameplayStart();
            void OnGameplayStop();
        }
    }
}