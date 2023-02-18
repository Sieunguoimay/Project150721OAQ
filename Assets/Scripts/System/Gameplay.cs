using System.Linq;
using Common.Activity;
using Gameplay;
using Gameplay.Board;
using Gameplay.GameInteract;

namespace System
{
    public interface IGameplay
    {
        void StartNewMatch();
    }

    public class Gameplay : IGameplay
    {
        private readonly IPieceDropper _dropper = new PieceDropper();
        private readonly PieceEater _eater = new();

        private Player[] _players;
        private Board _board;
        private GameInteractManager _interact;

        private Player CurrentPlayer { get; set; }

        private bool IsGameOver { get; set; }
        public bool IsPlaying { get; private set; }

        public ActivityQueue ActivityQueue { get; } = new();

        private IBoardStateDriver _concurrentBoardStateDriver;
        private IBoardStateDriver _boardStateDriver;
        private BoardActionExecutor _boardActionExecutor;
        private BoardActionExecutor[] _concurrentBoardActionExecutors;

        public void Setup(Player[] players, Board board, GameInteractManager interactManager)
        {
            _board = board;
            _players = players;
            _interact = interactManager;

            _dropper.Setup(_board.Tiles);

            IsPlaying = false;
            IsGameOver = false;
            NextPlayer();
            _interact.MoveEvent -= OnMove;
            _interact.MoveEvent += OnMove;
        }


        public void StartNewMatch()
        {
            IsPlaying = true;
            _interact.ShowTileChooser(_board.Sides[CurrentPlayer.Index].CitizenTiles);
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

        public void ClearGame()
        {
            IsGameOver = false;
            IsPlaying = false;
            _dropper.Cleanup();
            _eater.Cleanup();
            CurrentPlayer = null;
            _interact.ResetAll();
            _interact.MoveEvent -= OnMove;
        }

        #region PRIVATE_METHODS

        private void NextPlayer()
        {
            if (CurrentPlayer == null)
            {
                CurrentPlayer = _players[0];
            }
            else
            {
                CurrentPlayer.ReleaseTurn();
                CurrentPlayer = _players[(CurrentPlayer.Index + 1) % _players.Length];
            }

            CurrentPlayer.AcquireTurn();
        }


        private void OnMove(ITile tile, bool forward)
        {
            // _dropper.TakeAll(tile);
            // _dropper.SetMoveStartPoint(tile.TileIndex, forward);
            // _dropper.DropTillDawn((_, lastTile) =>
            // {
            //     if (!_eater.TryEat(_board.Tiles, CurrentPlayer.PieceBench, lastTile.TileIndex, forward, MakeDecision))
            //     {
            //         MakeDecision();
            //     }
            // });
            //
            // new MultiPieceDropper().DropConcurrently(_board.Tiles, CurrentPlayer.PieceBench, new[]
            // {
            //     tile.TileIndex, BoardTraveller.MoveNext(tile.TileIndex, _board.Tiles.Count, forward, 2),
            // }, MakeDecision, forward);
            
            Drop2TilesConcurrently(tile.TileIndex,
                BoardTraveller.MoveNext(tile.TileIndex, _board.Tiles.Count, forward, 2), forward);
        }

        private void DropSingleTile(int tileIndex, bool direction)
        {
            _boardActionExecutor.Initialize(CreateBoardActionExecutorArgument(tileIndex,direction));
            _boardStateDriver.NextAction();
        }

        private void Drop2TilesConcurrently(int tileIndex1, int tileIndex2, bool direction)
        {
            _concurrentBoardActionExecutors[0].Initialize(CreateBoardActionExecutorArgument(tileIndex1,direction));
            _concurrentBoardActionExecutors[1].Initialize(CreateBoardActionExecutorArgument(tileIndex2,direction));
            _concurrentBoardStateDriver.NextAction();
        }

        private BoardActionExecutor.Argument CreateBoardActionExecutorArgument(int tileIndex, bool direction)
        {
            return new()
            {
                board = _board,
                bench = CurrentPlayer.PieceBench,
                direction = direction,
                singleActionDuration = 1,
                tileIndex = tileIndex
            };
        }
        private void OnBoardStateDriverEnd(IBoardStateDriver obj)
        {
            MakeDecision();
        }

        private void MakeDecision()
        {
            var allMandarinTilesEmpty = _board.Sides.All(tg => tg.MandarinTile.HeldPieces.Count <= 0);
            if (allMandarinTilesEmpty)
            {
                GameOver();
                return;
            }

            NextPlayer();

            var tileGroup = _board.Sides[CurrentPlayer.Index];
            var isNewPlayerAllEmpty = tileGroup.CitizenTiles.All(t => t.HeldPieces.Count <= 0);
            if (isNewPlayerAllEmpty)
            {
                if (CurrentPlayer.PieceBench.HeldPieces.Count <= 0)
                {
                    GameOver();
                    return;
                }

                //Take back pieces to board
                _dropper.Take(CurrentPlayer.PieceBench, tileGroup.CitizenTiles.Length);
                _dropper.SetMoveStartPoint(tileGroup.MandarinTile.TileIndex, true);
                _dropper.DropOnce(_ => { _interact.ShowTileChooser(_board.Sides[CurrentPlayer.Index].CitizenTiles); });
                return;
            }

            _interact.ShowTileChooser(_board.Sides[CurrentPlayer.Index].CitizenTiles);
        }

        private void GameOver()
        {
            EvaluateWinner();

            if (IsGameOver) return;

            IsGameOver = true;
            IsPlaying = false;
        }

        private void EvaluateWinner()
        {
            for (var i = 0; i < _players.Length; i++)
            {
                foreach (var tile in _board.Sides[i].CitizenTiles)
                {
                    // _players[i].PieceBench.HeldPieces.AddRange(tile.HeldPieces);
                    tile.Clear();
                }

                // var sum = _players[i].PieceBench.HeldPieces.Sum(p => p is Citizen ? 1 : 10);

                // _perMatchData.SetPlayerScore(i, sum);
            }

            // TellWinner(_perMatchData.PlayerScores);
        }

        // private static void TellWinner(IReadOnlyList<Currency> scores)
        // {
        //     if (scores[0].Get() > scores[1].Get())
        //     {
        //         Debug.Log("Player 1 win! " + scores[0].Get() + " - " + scores[1].Get());
        //     }
        //     else if (scores[0].Get() < scores[1].Get())
        //     {
        //         Debug.Log("Player 2 win! " + scores[0].Get() + " - " + scores[1].Get());
        //     }
        //     else
        //     {
        //         Debug.Log("Draw! " + scores[0].Get() + " - " + scores[1].Get());
        //     }
        // }

        #endregion
    }
}