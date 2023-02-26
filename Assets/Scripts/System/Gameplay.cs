using System.Linq;
using Gameplay;
using Gameplay.Board;
using Gameplay.GameInteract;
using Gameplay.Player;

namespace System
{
    public interface IGameplay
    {
        void Initialize();
        void Cleanup();
        void Start();
        event Action<IGameplay> GameOverEvent;
    }

    public class Gameplay : IGameplay
    {
        private readonly IPlayerManager _playersManager;
        private readonly IBoardManager _boardManager;
        private readonly IPlayerInteract _playerInteract;

        private Board _board;
        private DropRunner _dropRunner;

        public Gameplay(IPlayerManager playersManager, IBoardManager boardManager, IPlayerInteract playerInteract)
        {
            _playersManager = playersManager;
            _boardManager = boardManager;
            _playerInteract = playerInteract;

            _playerInteract.ResultEvent -= OnPlayerInteractResult;
            _playerInteract.ResultEvent += OnPlayerInteractResult;
        }

        ~Gameplay()
        {
            _playerInteract.ResultEvent -= OnPlayerInteractResult;
        }

        public void Initialize()
        {
            _board = _boardManager.Board;
            _dropRunner = new DropRunner(this, new DropRunner.MoveStartingDataCreator(_playersManager), _board);
        }

        public void Cleanup()
        {
            _dropRunner.CleanUp();
        }

        public void Start()
        {
            MakeDecision();
        }

        public event Action<IGameplay> GameOverEvent;

        #region PRIVATE_METHODS

        private void OnPlayerInteractResult(PlayerInteractResult playerInteractResult)
        {
            var index = playerInteractResult.SelectedTile.TileIndex;
            var nextIndex = BoardTraveller.MoveNext(index, _board.Tiles.Count, playerInteractResult.Direction, 2);
            _dropRunner.Drop2TilesConcurrently(index, nextIndex, playerInteractResult.Direction);
        }

        public void MakeDecision()
        {
            var anyMandarinTilesHasPieces = _board.Sides.Any(tg =>
                tg.MandarinTile.Mandarin != null || tg.MandarinTile.HeldPieces.Count > 0);
            if (anyMandarinTilesHasPieces)
            {
                _playersManager.NextPlayer();
                CheckTilesOnCurrentPlayerSide();
            }
            else
            {
                GameOver();
            }
        }

        private void CheckTilesOnCurrentPlayerSide()
        {
            var anyTileHasPieces = _board.Sides[_playersManager.CurrentPlayer.Index].CitizenTiles
                .Any(t => t.HeldPieces.Count > 0);
            if (anyTileHasPieces)
            {
                ContinuePlaying();
            }
            else
            {
                CheckCurrentPlayerBench();
            }
        }

        private void ContinuePlaying()
        {
            _playerInteract.Show();
        }

        private void CheckCurrentPlayerBench()
        {
            var anyPieceOnBench = _playersManager.CurrentPlayer.PieceBench.HeldPieces.Count > 0;
            if (anyPieceOnBench)
            {
                TakePiecesBackToBoard();
            }
            else
            {
                GameOver();
            }
        }

        private void TakePiecesBackToBoard()
        {
            //Take back pieces to board
            // _dropper.Take(CurrentPlayer.PieceBench, tileGroup.CitizenTiles.Length);
            // _dropper.SetMoveStartPoint(tileGroup.MandarinTile.TileIndex, true);
            // _dropper.DropOnce(_ => { _interact.ShowTileChooser(_board.Sides[CurrentPlayer.Index].CitizenTiles); });
        }

        private void GameOver()
        {
            // EvaluateWinner();
            GameOverEvent?.Invoke(this);
        }

        #endregion
    }

    public class DropRunner
    {
        private readonly Gameplay _gameplay;
        private readonly MoveStartingDataCreator _moveStartingDataCreator;

        private readonly IBoardStateDriver _concurrentBoardStateDriver;
        private readonly IBoardStateDriver _boardStateDriver;

        private readonly MoveMaker _moveMaker;
        private readonly MoveMaker[] _twoMoveMakers;

        public DropRunner(Gameplay gameplay, MoveStartingDataCreator moveStartingDataCreator, Board board)
        {
            _gameplay = gameplay;
            _moveStartingDataCreator = moveStartingDataCreator;

            _twoMoveMakers = new MoveMaker[] {new(board, 1), new(board, 1)};
            _moveMaker = new MoveMaker(board, 1);

            _boardStateDriver = new BoardStateMachine(_moveMaker);
            _concurrentBoardStateDriver = new MultiBoardStateMachine(_twoMoveMakers);

            _boardStateDriver.EndEvent += OnBoardStateDriverEnd;
            _concurrentBoardStateDriver.EndEvent += OnBoardStateDriverEnd;
        }

        public void CleanUp()
        {
            _boardStateDriver.EndEvent -= OnBoardStateDriverEnd;
            _concurrentBoardStateDriver.EndEvent -= OnBoardStateDriverEnd;
        }

        private void OnBoardStateDriverEnd(IBoardStateDriver obj)
        {
            _gameplay.MakeDecision();
        }

        public void DropSingleTile(int tileIndex, bool direction)
        {
            _moveMaker.Initialize(_moveStartingDataCreator.Create(tileIndex, direction));
            _boardStateDriver.NextAction();
        }

        public void Drop2TilesConcurrently(int tileIndex1, int tileIndex2, bool direction)
        {
            _twoMoveMakers[0].Initialize(_moveStartingDataCreator.Create(tileIndex1, direction));
            _twoMoveMakers[1].Initialize(_moveStartingDataCreator.Create(tileIndex2, direction));
            _concurrentBoardStateDriver.NextAction();
        }

        public class MoveStartingDataCreator
        {
            private readonly IPlayerManager _playersManager;

            public MoveStartingDataCreator(IPlayerManager playersManager)
            {
                _playersManager = playersManager;
            }

            public MoveMaker.MoveConfig Create(int tileIndex, bool direction)
            {
                return new MoveMaker.MoveConfig
                {
                    Bench = _playersManager.GetCurrentPlayerBench(),
                    Direction = direction,
                    StartingTileIndex = tileIndex
                };
            }
        }
    }
}