using System.Linq;
using Gameplay.CoreGameplay.Controllers;
using Gameplay.CoreGameplay.Interactors.Simulation;
using Gameplay.GameInteract;
using Gameplay.Helpers;
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
            ShowInteract();
        }

        public event Action<Gameplay> GameOverEvent;

        #region PRIVATE_METHODS

        private bool AnyMandarinTileHasPieces()
        {
            return _boardStatePresenter.BoardStateViewData.AnyMandarinTileHasPieces;
        }

        private bool AnyTileOnCurrentSideHasPieces()
        {
            return _turnTeller.CurrentTurn.AnyCitizenTileHasPieces();
        }

        private bool AnyPiecesAvailableOnBenchOfCurrentSide()
        {
            return _turnTeller.CurrentTurn.AnyPieceOnBench();
        }

        private void TakePiecesBackToBoard()
        {
        }

        private void ShowInteract()
        {
            _playerInteract.Show();
        }

        private void GameOver()
        {
            // EvaluateWinner();
            GameOverEvent?.Invoke(this);
        }

        private void UpdateTurn()
        {
            _turnTeller.NextTurn();
        }

        private void OnPlayerInteractResult(PlayerInteractResult playerInteractResult)
        {
            var index = playerInteractResult.SelectedTile.TileIndex;
            _dropRunner.DropSingleTile(index, playerInteractResult.Direction);
        }

        private class DropRunner
        {
            private readonly Gameplay _gameplay;
            private readonly BranchingLogic _logic;

            public DropRunner(Gameplay gameplay)
            {
                _gameplay = gameplay;
                _gameplay._coreGameplayVisualPresenter.MovingRunner.AllMovingStepsExecutedEvent -= OnAllMovingStepsDone;
                _gameplay._coreGameplayVisualPresenter.MovingRunner.AllMovingStepsExecutedEvent += OnAllMovingStepsDone;
                _logic = new BranchingLogic(gameplay);
            }

            public void CleanUp()
            {
                _gameplay._coreGameplayVisualPresenter.MovingRunner.AllMovingStepsExecutedEvent -= OnAllMovingStepsDone;
            }

            private void OnAllMovingStepsDone(PiecesMovingRunner obj)
            {
                _gameplay._controller.RequestRefresh(_gameplay._boardStatePresenter);

                BoardStateMatchVisualVerify.Verify(_gameplay._boardStatePresenter.BoardStateViewData, _gameplay._board);

                _logic.Branch();
            }

            public void DropSingleTile(int tileIndex, bool direction)
            {
                _gameplay._controller.RunSimulation(new MoveSimulationInputData
                {
                    Direction = direction,
                    SideIndex = _gameplay._turnTeller.CurrentTurn.SideIndex,
                    StartingTileIndex = tileIndex
                });
            }
        }

        private class BranchingLogic
        {
            private readonly Gameplay _gameplay;

            public BranchingLogic(Gameplay gameplay)
            {
                _gameplay = gameplay;
            }

            public void Branch()
            {
                CheckPiecesInMandarinTiles();
            }

            private void CheckPiecesInMandarinTiles()
            {
                if (_gameplay.AnyMandarinTileHasPieces())
                {
                    _gameplay.UpdateTurn();
                    CheckPiecesOnCurrentSide();
                }
                else
                {
                    _gameplay.GameOver();
                }
            }

            private void CheckPiecesOnCurrentSide()
            {
                if (_gameplay.AnyTileOnCurrentSideHasPieces())
                {
                    _gameplay.ShowInteract();
                }
                else
                {
                    CheckBenchOnCurrentSideForPieces();
                }
            }

            private void CheckBenchOnCurrentSideForPieces()
            {
                if (_gameplay.AnyPiecesAvailableOnBenchOfCurrentSide())
                {
                    _gameplay.TakePiecesBackToBoard();
                }
                else
                {
                    _gameplay.GameOver();
                }
            }
        }

        #endregion
    }
}