using System.Linq;
using Gameplay.CoreGameplay.Interactors.Simulation;
using Gameplay.GameInteract;
using Gameplay.Helpers;
using Gameplay.PlayTurn;
using Gameplay.Visual.Board;

namespace System
{
    public class Gameplay
    {
        private readonly IPlayTurnTeller _turnTeller;
        private readonly IPlayerInteract _playerInteract;
        private readonly GridLocator _gridLocator;

        public IGameplayContainer Container { get; private set; }
        private DropRunner _dropRunner;

        public Gameplay(IPlayTurnTeller turnTeller, IPlayerInteract playerInteract, GridLocator gridLocator)
        {
            _turnTeller = turnTeller;
            _playerInteract = playerInteract;
            _gridLocator = gridLocator;

            _playerInteract.ResultEvent -= OnPlayerInteractResult;
            _playerInteract.ResultEvent += OnPlayerInteractResult;
        }

        ~Gameplay()
        {
            _playerInteract.ResultEvent -= OnPlayerInteractResult;
        }

        public void Initialize(IGameplayContainer container)
        {
            Container = container;
            _dropRunner = new DropRunner(this, new DropRunner.MoveStartingDataCreator(_turnTeller), Container.Board, _gridLocator);
        }

        public void Cleanup()
        {
            _dropRunner.CleanUp();
        }

        public void Start()
        {
            MakeDecision();
        }

        public event Action<Gameplay> GameOverEvent;

        #region PRIVATE_METHODS

        private void OnPlayerInteractResult(PlayerInteractResult playerInteractResult)
        {
            var index = playerInteractResult.SelectedTile.TileIndex;
            // var nextIndex = BoardTraveller.MoveNext(index, _board.Tiles.Count, playerInteractResult.Direction, 2);
            _dropRunner.DropSingleTile(index, playerInteractResult.Direction);
        }

        public void MakeDecision()
        {
            var anyMandarinTilesHasPieces = Container.Board.Sides.Any(tg =>
                tg.MandarinTile.Mandarin != null || tg.MandarinTile.HeldPieces.Count > 0);
            if (anyMandarinTilesHasPieces)
            {
                _turnTeller.NextTurn();
                CheckTilesOnCurrentPlayerSide();
            }
            else
            {
                GameOver();
            }
        }

        private void CheckTilesOnCurrentPlayerSide()
        {
            var anyTileHasPieces = _turnTeller.CurrentTurn.BoardSide.CitizenTiles
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
            var anyPieceOnBench = _turnTeller.CurrentTurn.PieceBench.HeldPieces.Count > 0;
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

        public DropRunner(Gameplay gameplay, MoveStartingDataCreator moveStartingDataCreator, Board board, GridLocator gridLocator)
        {
            _gameplay = gameplay;
            _moveStartingDataCreator = moveStartingDataCreator;

            _twoMoveMakers = new MoveMaker[] {new(board, 1, gridLocator), new(board, 1, gridLocator)};
            _moveMaker = new MoveMaker(board, 1, gridLocator);

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
            private readonly IPlayTurnTeller _turnTeller;

            public MoveStartingDataCreator(IPlayTurnTeller turnTeller)
            {
                _turnTeller = turnTeller;
            }

            public MoveMaker.MoveConfig Create(int tileIndex, bool direction)
            {
                return new()
                {
                    Bench = _turnTeller.CurrentTurn.PieceBench,
                    Direction = direction,
                    StartingTileIndex = tileIndex
                };
            }
        }
    }
}