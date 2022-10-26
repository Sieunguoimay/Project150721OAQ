using System.Linq;
using Common;
using DG.Tweening;
using Framework.Resolver;
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
    public class GameManager : MonoBehaviour, IInjectable, IBinding
    {
        private readonly Gameplay _gameplay = new();
        private readonly IMatchChooser _matchChooser = new MatchChooser();
        private readonly GameFlowManager _gameFlowManager = new();

        private BambooFamilyManager _bambooFamily;
        private PlayersManager _playersManager;
        private BoardManager _boardManager;
        private PieceManager _pieceManager;

        private IResolver _resolver;

        public void SelfBind(IBinder binder)
        {
            binder.Bind(_gameFlowManager);
            binder.Bind(_matchChooser);
        }

        public void SelfUnbind(IBinder binder)
        {
            binder.Unbind(_gameFlowManager);
            binder.Unbind(_matchChooser);
        }

        public void Inject(IResolver resolver)
        {
            _resolver = resolver;

            _playersManager = resolver.Resolve<PlayersManager>();
            _boardManager = resolver.Resolve<BoardManager>();
            _pieceManager = resolver.Resolve<PieceManager>();
            _bambooFamily = resolver.Resolve<BambooFamilyManager>();

            RayPointer.Instance.SetCamera(resolver.Resolve<CameraManager>().Camera);
        }

        private void OnEnable()
        {
            _matchChooser.OnMatchOptionChanged += OnMatchChooserResult;
        }

        private void OnDisable()
        {
            _matchChooser.OnMatchOptionChanged -= OnMatchChooserResult;
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
            var playerNum = _matchChooser.MatchOption.PlayerNum;
            var tilesPerGroup = _matchChooser.MatchOption.TilesPerGroup;

            _boardManager.CreateBoard(playerNum, tilesPerGroup);

            _playersManager.FillWithFakePlayers(playerNum);
            _playersManager.CreatePieceBench(_boardManager.Board);

            _gameplay.Setup(_playersManager, _boardManager.Board, _pieceManager,
                _resolver.Resolve<GameInteractManager>(), _resolver);

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