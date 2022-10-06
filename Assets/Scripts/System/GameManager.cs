using System.Linq;
using Common;
using Common.ResolveSystem;
using DG.Tweening;
using Gameplay;
using Gameplay.BambooStick;
using Gameplay.Board;
using Gameplay.Board.BoardDrawing;
using Gameplay.Piece;
using SNM;
using UnityEngine;

namespace System
{
    public class GameManager : MonoBehaviour, IInjectable
    {
        private readonly Gameplay _gameplay = new();
        private readonly MatchChooser _matchChooser = new();
        private GameFlowManager _gameFlowManager;
        private PlayersManager _playersManager;
        private BoardManager _boardManager;
        private PieceManager _pieceManager;
        private BambooFamilyManager _bambooFamily;

        public void Bind(IResolver resolver)
        {
            _gameFlowManager = new GameFlowManager();

            resolver.Bind(_gameFlowManager);
            resolver.Bind<IMatchChooser>(_matchChooser);
        }

        public void Setup(IResolver resolver)
        {
            _playersManager = resolver.Resolve<PlayersManager>();
            _boardManager = resolver.Resolve<BoardManager>();
            _pieceManager = resolver.Resolve<PieceManager>();
            _bambooFamily = resolver.Resolve<BambooFamilyManager>();

            var cam = resolver.Resolve<CameraManager>().Camera;
            RayPointer.Instance.SetCamera(cam);

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
        
        private void OnMatchChooserResult()
        {
            GenerateMatch();
            StartGame();
        }
        
        public void GenerateMatch()
        {
            var playerNum = _matchChooser.PlayerNum;
            var tilesPerGroup = _matchChooser.TilesPerGroup;
            
            _boardManager.SetBoardByTileGroupNum(playerNum, _matchChooser.TilesPerGroup);
            
            _playersManager.FillWithFakePlayers(playerNum);
            
            _playersManager.CreatePieceBench(_boardManager.Board);
              
            _gameplay.Setup(_playersManager, _boardManager.Board, _pieceManager);

            _pieceManager.SpawnPieces(playerNum, tilesPerGroup);
            
            // _gameFlowManager.ChangeState(GameFlowManager.GameState.BeforeGameplay);

            // _boardSketcher.Sketch(_boardManager.Board);
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