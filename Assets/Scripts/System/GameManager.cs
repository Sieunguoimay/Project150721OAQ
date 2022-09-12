using System.Linq;
using Common;
using Common.ResolveSystem;
using DG.Tweening;
using Gameplay;
using Gameplay.Board;
using Gameplay.Piece;
using SNM;
using UnityEngine;

namespace System
{
    public class GameManager : MonoBehaviour, IGameFlowHandler
    {
        [SerializeField] public PieceManager pieceManager;
        [SerializeField] public PlayersManager playersManager;
        [SerializeField] public BoardManager boardManager;
        [SerializeField] public TileSelector tileSelector;
        [SerializeField] public UIManager uiManager;
        [SerializeField] public CameraManager cameraManager;
        [SerializeField] public BoardSketcher boardSketcher;

        private readonly Gameplay _gameplay = new();
        private readonly MatchChooser _matchChooser = new();
        private readonly GameEvents _events = new();
        private GameFlowManager _gameFlowManager;

        private IInjectable[] _injectables;
        private readonly Resolver _resolver = new();

        private void Awake()
        {
            _gameFlowManager = new GameFlowManager(this);

            Bind();
            _injectables = GetComponentsInChildren<IInjectable>()
                .Concat(uiManager.GetComponentsInChildren<IInjectable>())
                .Concat(new[] {RayPointer.Instance}).ToArray();
            foreach (var injectable in _injectables)
            {
                injectable.Inject(_resolver);
            }
        }

        private void Start()
        {
            OnSetup();
        }

        private void OnDestroy()
        {
            OnCleanup();
            Unbind();
        }

        private void Bind()
        {
            _resolver.Bind<IMatchChooser>(_matchChooser);
            _resolver.Bind(cameraManager);
            _resolver.Bind(tileSelector);
            _resolver.Bind<IGameEvents>(_events);
            _resolver.Bind(_gameFlowManager);
        }

        private void Unbind()
        {
            _resolver.Unbind<IMatchChooser>(_matchChooser);
            _resolver.Unbind(cameraManager);
            _resolver.Unbind(tileSelector);
            _resolver.Unbind<IGameEvents>(_events);
            _resolver.Unbind(_gameFlowManager);
        }

        private void OnSetup()
        {
            // _matchChooser.OnMatchOptionChanged += GenerateMatch;
        }


        private void OnCleanup()
        {
            // _matchChooser.OnMatchOptionChanged -= GenerateMatch;
            _gameplay.TearDown();
        }

        public void GenerateMatch()
        {
            boardManager.SetBoardByTileGroupNum(_matchChooser.PlayerNum, _matchChooser.TilesPerGroup);

            playersManager.FillWithFakePlayers(_matchChooser.PlayerNum);
            playersManager.CreatePieceBench(boardManager.Board);

            pieceManager.SpawnPieces(_matchChooser.PlayerNum, _matchChooser.TilesPerGroup);

            _gameplay.Setup(playersManager.Players, boardManager.Board, pieceManager);

            _gameFlowManager.ChangeState(GameFlowManager.GameState.BeforeGameplay);

            boardSketcher.Sketch(boardManager.Board);
        }

        public void StartGame()
        {
            _gameplay.StartNewMatch();
            _events.OnStart();
        }

        public void ReplayMatch()
        {
            _gameplay.ResetGame();
            playersManager.ResetAll();
            pieceManager.ResetAll();
            boardManager.ResetAll();
            tileSelector.ResetAll();
            _events.OnReplayMatch();
        }

        public void ResetGame()
        {
            _gameplay.ResetGame();
            _gameplay.TearDown();
            playersManager.ResetAll();
            pieceManager.ResetAll();
            boardManager.ResetAll();
            tileSelector.ResetAll();

            foreach (var p in pieceManager.Pieces)
            {
                DOTween.Kill(p);
                p.StopAllCoroutines();
                Destroy(p.gameObject);
            }

            foreach (var t in boardManager.Board.Tiles)
            {
                (t as Tile)?.TearDown();
                Destroy((t as MonoBehaviour)?.gameObject);
            }

            pieceManager.Pieces = null;
            _events.OnReset();
        }

        public interface IGameEvents
        {
            event Action Start;
            event Action Reset;
            event Action ReplayMatch;
        }

        private sealed class GameEvents : IGameEvents
        {
            public event Action Start;
            public event Action Reset;
            public event Action ReplayMatch;

            public void OnStart()
            {
                Start?.Invoke();
            }

            public void OnReset()
            {
                Reset?.Invoke();
            }

            public void OnReplayMatch()
            {
                ReplayMatch?.Invoke();
            }
        }
    }
}