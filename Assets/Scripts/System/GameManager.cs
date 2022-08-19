using Gameplay;
using Gameplay.Board;
using Gameplay.Piece;
using SNM;
using UnityEngine;

namespace System
{
    public class GameManager : MonoBehaviour, IManager
    {
        [SerializeField] public PieceManager pieceManager;
        [SerializeField] public PlayersManager playersManager;
        [SerializeField] public BoardManager boardManager;

        private readonly Gameplay _gameplay = new();
        private readonly MatchOption _matchOption = new();

        private void Start()
        {
            OnInitialize();
            OnSetup();
        }

        private void OnDestroy()
        {
            OnCleanup();
        }

        private void Update()
        {
            if (!_gameplay.IsPlaying && Input.GetMouseButton(0))
            {
                OnGameStart();
            }

            if (Input.GetKeyUp(KeyCode.Return))
            {
                OnGameReset();
            }
        }

        #region IManager

        public void OnInitialize()
        {
        }

        public void OnSetup()
        {
            boardManager.SetBoardByTileGroupNum(_matchOption.PlayerNum, _matchOption.TilesPerGroup);

            playersManager.FillWithFakePlayers(_matchOption.PlayerNum);
            playersManager.CreatePieceBench(boardManager.Board);

            pieceManager.SpawnPieces(_matchOption.PlayerNum, _matchOption.TilesPerGroup);

            _gameplay.Setup(playersManager.Players, boardManager.Board, pieceManager);
        }

        public void OnCleanup()
        {
            _gameplay.TearDown();
        }

        public void OnGameStart()
        {
            _gameplay.StartNewMatch();
        }

        public void OnGameEnd()
        {
        }

        public void OnGameReset()
        {
            _gameplay.ResetGame();
        }

        #endregion
    }
}