using System.Linq;
using Common;
using Common.ResolveSystem;
using DG.Tweening;
using Gameplay;
using Gameplay.BambooStick;
using Gameplay.Board;
using Gameplay.Board.BoardDrawing;
using Gameplay.GameInteract;
using Gameplay.Piece;
using SNM;
using UnityEngine;

namespace System
{
    public class GameManager : MonoBehaviour, IInjectable
    {
        private readonly Gameplay _gameplay = new();
        private readonly MatchChooser _matchChooser = new();
        private readonly GameFlowManager _gameFlowManager = new();

        private BambooFamilyManager _bambooFamily;
        private PlayersManager _playersManager;
        private BoardManager _boardManager;
        private PieceManager _pieceManager;

        private IResolver _resolver;

        public void Bind(IResolver resolver)
        {
            _resolver = resolver;

            resolver.Bind(_gameFlowManager);
            resolver.Bind<IMatchChooser>(_matchChooser);
        }

        public void Setup(IResolver resolver)
        {
            _playersManager = resolver.Resolve<PlayersManager>();
            _boardManager = resolver.Resolve<BoardManager>();
            _pieceManager = resolver.Resolve<PieceManager>();
            _bambooFamily = resolver.Resolve<BambooFamilyManager>();

            RayPointer.Instance.SetCamera(resolver.Resolve<CameraManager>().Camera);

            _matchChooser.OnMatchOptionChanged += OnMatchChooserResult;
        }

        public void TearDown()
        {
            OnCleanup();
        }

        public void Unbind(IResolver resolver)
        {
            resolver.Unbind(_gameFlowManager);
            resolver.Unbind<IMatchChooser>(_matchChooser);
        }

        private void OnCleanup()
        {
            _gameplay.TearDown();
        }

        private void Update()
        {
            _gameplay.ActivityQueue.Update(Time.deltaTime);
        }

        private void OnMatchChooserResult()
        {
            GenerateMatch();
            StartGame();
        }

        public void GenerateMatch()
        {
            var playerNum = _matchChooser.PlayerNum;
            var tilesPerGroup = _matchChooser.TilesPerGroup;

            _boardManager.CreateBoard(playerNum, _matchChooser.TilesPerGroup);

            _playersManager.FillWithFakePlayers(playerNum);
            _playersManager.CreatePieceBench(_boardManager.Board);

            _gameplay.Setup(_playersManager, _boardManager.Board, _pieceManager,
                _resolver.Resolve<GameInteractManager>());

            _pieceManager.SpawnPieces(playerNum, tilesPerGroup);

            _bambooFamily.BeginAnimSequence();
        }

        public void StartGame()
        {
            _gameplay.StartNewMatch();
        }

        public void ReplayMatch()
        {
            _gameplay.ResetGame();
        }

        public void ResetGame()
        {
            _gameplay.ResetGame();
            _gameplay.TearDown();
        }
    }
}