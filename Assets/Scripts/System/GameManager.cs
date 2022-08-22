using System.Linq;
using Common;
using Common.ResolveSystem;
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
                OnGameReset();
            }
        }

        private void Bind()
        {
            _resolver.Bind<IMatchOption>(_matchOption);
            _resolver.Bind(cameraManager);
            _resolver.Bind(tileSelector);
        }

        private void Unbind()
        {
            _resolver.Unbind<IMatchOption>(_matchOption);
            _resolver.Unbind(cameraManager);
            _resolver.Unbind(tileSelector);
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
        }

        private void OnGameReset()
        {
            _gameplay.ResetGame();
        }
    }
}