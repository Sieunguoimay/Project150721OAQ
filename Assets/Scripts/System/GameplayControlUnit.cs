﻿using System.Collections;
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
            StartCoroutine(StartGameCoroutine());
        }

        private IEnumerator StartGameCoroutine()
        {
            yield return StartCoroutine(GenerateMatch(_stageSelector.SelectedStage));

            _gameplay.StartNewMatch();
        }

        public IEnumerator GenerateMatch(IStage stage)
        {
            var done = false;
            _boardManager.CreateBoard(stage.Data.PlayerNum, stage.Data.TilesPerGroup);

            _playersManager.FillWithFakePlayers(stage.Data.PlayerNum);
            _playersManager.CreatePieceBench(_boardManager.Board);

            _gameplay.Setup(_playersManager.Players, _boardManager.Board, _pieceManager, _interact);

            _pieceManager.SpawnPieces(stage.Data.PlayerNum, stage.Data.TilesPerGroup, stage.Data.NumCitizensInTile);
            _pieceManager.ReleasePieces(() => { done = true; }, _boardManager.Board);

            _bambooFamily.BeginAnimSequence();

            yield return new WaitUntil(() => done);
        }

        public void ClearGame()
        {
            _bambooFamily.ResetAll();
            _pieceManager.DeletePieces();
            _gameplay.ClearGame();
            _playersManager.DeletePlayers();
            _boardManager.DeleteBoard();
        }
    }
}