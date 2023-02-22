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
            _dropRunner = new DropRunner(this, new DropRunner.ActionArgumentCreator(_playersManager, _board));
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
            _dropRunner.Drop2TilesConcurrently(playerInteractResult.SelectedTile.TileIndex,
                BoardTraveller.MoveNext(playerInteractResult.SelectedTile.TileIndex, _board.Tiles.Count, playerInteractResult.Direction, 2), playerInteractResult.Direction);
        }

        public void MakeDecision()
        {
            var anyMandarinTilesHasPieces = _board.Sides.Any(tg => tg.MandarinTile.Mandarin != null || tg.MandarinTile.HeldPieces.Count > 0);
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
            var anyTileHasPieces = _board.Sides[_playersManager.CurrentPlayer.Index].CitizenTiles.Any(t => t.HeldPieces.Count > 0);
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

        private readonly BoardActionExecutor _boardActionExecutor;
        private readonly BoardActionExecutor[] _concurrentBoardActionExecutors;

        public DropRunner(Gameplay gameplay, ActionArgumentCreator argumentCreator)
        {
            _gameplay = gameplay;
            _argumentCreator = argumentCreator;

            _concurrentBoardActionExecutors = new BoardActionExecutor[2];
            for (var i = 0; i < _concurrentBoardActionExecutors.Length; i++)
            {
                _concurrentBoardActionExecutors[i] = new BoardActionExecutor();
            }

            _boardActionExecutor = new BoardActionExecutor();
            _boardStateDriver = new BoardStateDriver(_boardActionExecutor);
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
            _boardActionExecutor.Initialize(_argumentCreator.Create(tileIndex, direction));
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
            private readonly Board _board;

            public ActionArgumentCreator(IPlayerManager playersManager, Board board)
            {
                _playersManager = playersManager;
                _board = board;
            }

            public BoardActionExecutor.Argument Create(int tileIndex, bool direction)
            {
                return new()
                {
                    board = _board,
                    bench = _playersManager.GetCurrentPlayerBench(),
                    direction = direction,
                    singleActionDuration = 1,
                    tileIndex = tileIndex
                };
            }
        }
    }
}