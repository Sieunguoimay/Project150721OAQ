using System.Linq;
using Gameplay;
using Gameplay.Board;
using Gameplay.GameInteract;

namespace System
{
    public interface IGameplay
    {
        void Initialize();
        void Cleanup();
        void Start();
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
            _dropRunner = new DropRunner(this, new DropRunner.ActionArgumentCreator(_playersManager), _board);
        }

        public void Cleanup()
        {
            _dropRunner.CleanUp();
        }

        public void Start()
        {
            MakeDecision();
        }

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
        }

        #endregion
    }

    public class DropRunner
    {
        private readonly Gameplay _gameplay;
        private readonly ActionArgumentCreator _argumentCreator;

        private readonly IBoardStateDriver _concurrentBoardStateDriver;
        private readonly IBoardStateDriver _boardStateDriver;

        private readonly MoveMaker _moveMaker;
        private readonly MoveMaker[] _concurrentBoardActionExecutors;

        public DropRunner(Gameplay gameplay, ActionArgumentCreator argumentCreator, Board board)
        {
            _gameplay = gameplay;
            _argumentCreator = argumentCreator;

            _concurrentBoardActionExecutors = new MoveMaker[2];

            for (var i = 0; i < _concurrentBoardActionExecutors.Length; i++)
            {
                _concurrentBoardActionExecutors[i] = new MoveMaker(board, 1);
            }

            _moveMaker = new MoveMaker(board, 1);

            _boardStateDriver = new BoardStateDriver(_moveMaker);
            _concurrentBoardStateDriver = new ConcurrentBoardStateDriver(_concurrentBoardActionExecutors);

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
            _moveMaker.Initialize(_argumentCreator.Create(tileIndex, direction));
            _boardStateDriver.NextAction();
        }

        public void Drop2TilesConcurrently(int tileIndex1, int tileIndex2, bool direction)
        {
            _concurrentBoardActionExecutors[0].Initialize(_argumentCreator.Create(tileIndex1, direction));
            _concurrentBoardActionExecutors[1].Initialize(_argumentCreator.Create(tileIndex2, direction));
            _concurrentBoardStateDriver.NextAction();
        }

        public class ActionArgumentCreator
        {
            private readonly IPlayerManager _playersManager;

            public ActionArgumentCreator(IPlayerManager playersManager)
            {
                _playersManager = playersManager;
            }

            public MoveMaker.Argument Create(int tileIndex, bool direction)
            {
                return new MoveMaker.Argument
                {
                    // Board = _board,
                    Bench = _playersManager.GetCurrentPlayerBench(),
                    Direction = direction,
                    // SingleActionDuration = 1,
                    StartingTileIndex = tileIndex
                };
            }
        }
    }
}