using System.Linq;
using Gameplay.CoreGameplay.Controllers;
using Gameplay.CoreGameplay.Interactors.Simulation;
using Gameplay.GameInteract;
using Gameplay.Helpers;
using Gameplay.Player;
using Gameplay.PlayTurn;
using Gameplay.Visual;
using Gameplay.Visual.Board;

namespace System
{
    public class Gameplay
    {
        private readonly IPlayTurnTeller _turnTeller;
        private readonly IPlayerInteract _playerInteract;

        private Board _board;
        private DropRunner _dropRunner;
        private readonly BoardStatePresenter _boardStatePresenter;
        private readonly ICoreGameplayController _controller;
        private readonly CoreGameplayVisualPresenter _coreGameplayVisualPresenter;

        public Gameplay(IPlayTurnTeller turnTeller, IPlayerInteract playerInteract, 
            BoardStatePresenter boardStatePresenter, ICoreGameplayController controller,
            CoreGameplayVisualPresenter coreGameplayVisualPresenter)
        {
            _turnTeller = turnTeller;
            _playerInteract = playerInteract;
            _boardStatePresenter = boardStatePresenter;
            _controller = controller;
            _coreGameplayVisualPresenter = coreGameplayVisualPresenter;

            _playerInteract.ResultEvent -= OnPlayerInteractResult;
            _playerInteract.ResultEvent += OnPlayerInteractResult;
        }

        public void Initialize(Board board)
        {
            _board = board;
            _dropRunner = new DropRunner(this);
        }

        public void Cleanup()
        {
            _dropRunner.CleanUp();
            _playerInteract.ResultEvent -= OnPlayerInteractResult;
        }

        public void Start()
        {
            MakeDecision();
        }

        public event Action<Gameplay> GameOverEvent;

        #region PRIVATE_METHODS

        public void MakeDecision()
        {
            var anyMandarinTilesHasPieces = _boardStatePresenter.BoardStateViewData.AnyMandarinTileHasPieces;
            if (anyMandarinTilesHasPieces)
            {
                _turnTeller.NextTurn();
                MakeDecisionBaseOnPiecesInBoardSide();
            }
            else
            {
                GameOver();
            }
        }

        private void MakeDecisionBaseOnPiecesInBoardSide()
        {
            var anyTileHasPieces = _turnTeller.CurrentTurn.AnyCitizenTileHasPieces();
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

        private void OnPlayerInteractResult(PlayerInteractResult playerInteractResult)
        {
            var index = playerInteractResult.SelectedTile.TileIndex;
            // var nextIndex = BoardTraveller.MoveNext(index, _board.Tiles.Count, playerInteractResult.Direction, 2);

            _dropRunner.DropSingleTile(index, playerInteractResult.Direction, _turnTeller.CurrentTurn.PieceBench);
        }

        private void CheckCurrentPlayerBench()
        {
            var anyPieceOnBench = _turnTeller.CurrentTurn.AnyPieceOnBench();
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

        private class DropRunner
        {
            private readonly Gameplay _gameplay;
            private bool _lock;

            public DropRunner(Gameplay gameplay)
            {
                _gameplay = gameplay;
                _gameplay._coreGameplayVisualPresenter.MovingRunner.AllMovingStepsExecutedEvent -= OnAllMovingStepsDone;
                _gameplay._coreGameplayVisualPresenter.MovingRunner.AllMovingStepsExecutedEvent += OnAllMovingStepsDone;
            }

            public void CleanUp()
            {
                _gameplay._coreGameplayVisualPresenter.MovingRunner.AllMovingStepsExecutedEvent -= OnAllMovingStepsDone;
            }

            private void OnAllMovingStepsDone(PiecesMovingRunner obj)
            {
                _gameplay._controller.RequestRefresh(_gameplay._boardStatePresenter);

                BoardStateMatchVisualVerify.Verify(_gameplay._boardStatePresenter.BoardStateViewData, _gameplay._board);

                _gameplay.MakeDecision();

                _lock = false;
            }

            public void DropSingleTile(int tileIndex, bool direction, PieceBench bench)
            {
                if (!_lock)
                {
                    _lock = true;
                    _gameplay._controller.RunSimulation(new MoveSimulationInputData
                    {
                        Direction = direction,
                        SideIndex = _gameplay._turnTeller.CurrentTurn.SideIndex,
                        StartingTileIndex = tileIndex
                    });
                }
            }
        }

        #endregion
    }
}