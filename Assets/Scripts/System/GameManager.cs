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
    public class GameManager : MonoBehaviour
    {
        [SerializeField] public PieceManager pieceManager;
        [SerializeField] public PlayersManager playersManager;
        [SerializeField] public BoardManager boardManager;
        [SerializeField] public TileSelector tileSelector;
        [SerializeField] public UIManager uiManager;
        [SerializeField] public CameraManager cameraManager;

        private readonly Gameplay _gameplay = new();
        private readonly MatchOption _matchOption = new();
        private readonly GameEvents _events = new();

        private IInjectable[] _injectables;
        private readonly Resolver _resolver = new();

        private bool _matchOptionChosen;

        private void Awake()
        {
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

        private void Update()
        {
            if (!_matchOptionChosen) return;
            if (!_gameplay.IsPlaying && Input.GetMouseButton(0))
            {
                OnGameStart();
            }

            if (Input.GetKeyUp(KeyCode.Return))
            {
                ReplayMatch();
            }
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                ResetGame();
            }
        }

        private void Bind()
        {
            _resolver.Bind<IMatchOption>(_matchOption);
            _resolver.Bind(cameraManager);
            _resolver.Bind(tileSelector);
            _resolver.Bind<IGameEvents>(_events);
        }

        private void Unbind()
        {
            _resolver.Unbind<IMatchOption>(_matchOption);
            _resolver.Unbind(cameraManager);
            _resolver.Unbind(tileSelector);
            _resolver.Unbind<IGameEvents>(_events);
        }

        private void OnSetup()
        {
            _matchOptionChosen = false;
            _matchOption.OnMatchOptionChanged += OnMatchOptionChanged;
        }


        private void OnCleanup()
        {
            _matchOption.OnMatchOptionChanged -= OnMatchOptionChanged;
            _gameplay.TearDown();
        }

        private void OnMatchOptionChanged()
        {
            boardManager.SetBoardByTileGroupNum(_matchOption.PlayerNum, _matchOption.TilesPerGroup);

            playersManager.FillWithFakePlayers(_matchOption.PlayerNum);
            playersManager.CreatePieceBench(boardManager.Board);

            pieceManager.SpawnPieces(_matchOption.PlayerNum, _matchOption.TilesPerGroup);

            _gameplay.Setup(playersManager.Players, boardManager.Board, pieceManager);

            _matchOptionChosen = true;
        }

        private void OnGameStart()
        {
            _gameplay.StartNewMatch();
            _events.OnStart();
        }

        private void ReplayMatch()
        {
            _gameplay.ResetGame();
            playersManager.ResetAll();
            pieceManager.ResetAll();
            boardManager.ResetAll();
            _events.OnReplayMatch();
        }

        private void ResetGame()
        {
            _gameplay.ResetGame();
            playersManager.ResetAll();
            pieceManager.ResetAll();
            boardManager.ResetAll();
            
            foreach (var p in pieceManager.Pieces)
            {
                DOTween.Kill(p);
                p.StopAllCoroutines();
                Destroy(p.gameObject);
            }

            pieceManager.Pieces = null;
            _matchOptionChosen = false;
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