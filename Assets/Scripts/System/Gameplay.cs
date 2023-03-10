using Gameplay.CoreGameplay.Controllers;
using Gameplay.CoreGameplay.Interactors.Simulation;
using Gameplay.GameInteract;
using Gameplay.PlayTurn;
using Gameplay.Visual;
using Gameplay.Visual.Presenters;
using Gameplay.Visual.Views;

namespace System
{
    public class Gameplay
    {
        private readonly IPlayTurnTeller _turnTeller;
        private readonly IPlayerInteract _playerInteract;

        private readonly BoardStatePresenter _boardStatePresenter;
        private readonly ICoreGameplayController _controller;
        private readonly PiecesMovingRunner _movingRunner;
        private readonly BoardVisualView _boardVisualView;
        private readonly BranchingLogic _logic;
        private readonly BoardStateMatchVisualVerify _verify;

        public Gameplay(IGameplayContainer container,
            IPlayerInteract playerInteract,
            BoardStatePresenter boardStatePresenter,
            ICoreGameplayController controller,
            BoardVisualView boardVisualView)
        {
            _turnTeller = container.PlayTurnTeller;
            _playerInteract = playerInteract;
            _boardStatePresenter = boardStatePresenter;
            _controller = controller;
            _movingRunner = container.MovingRunner;
            _boardVisualView = boardVisualView;

            _logic = new BranchingLogic(this, _boardStatePresenter.BoardStateView, _turnTeller);
            _verify = new BoardStateMatchVisualVerify(_boardStatePresenter.BoardStateView, _boardVisualView.BoardVisual);
            
            _playerInteract.ResultEvent -= OnPlayerInteractResult;
            _playerInteract.ResultEvent += OnPlayerInteractResult;

            _movingRunner.AllMovingStepsExecutedEvent -= OnAllMovingStepsDone;
            _movingRunner.AllMovingStepsExecutedEvent += OnAllMovingStepsDone;

            _boardVisualView.VisualReadyEvent -= OnBoardVisualReady;
            _boardVisualView.VisualReadyEvent += OnBoardVisualReady;
        }

        private void OnBoardVisualReady(BoardVisualView obj)
        {
            Start();
        }

        public void Cleanup()
        {
            _boardVisualView.VisualReadyEvent -= OnBoardVisualReady;
            _movingRunner.AllMovingStepsExecutedEvent -= OnAllMovingStepsDone;
            _playerInteract.ResultEvent -= OnPlayerInteractResult;
            _movingRunner.Cleanup();
        }

        public void Start()
        {
            // ShowInteract();
            _controller.RunGameplay();
        }

        public event Action<Gameplay> GameOverEvent;


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
            DropSingleTile(index, playerInteractResult.Direction);
        }

        private void OnAllMovingStepsDone(PiecesMovingRunner obj)
        {
            _controller.RequestRefresh(_boardStatePresenter);

            _verify.Verify();

            // _logic.Branch();
        }

        private void DropSingleTile(int tileIndex, bool direction)
        {
            _controller.RunSimulation(new MoveSimulationInputData
            {
                Direction = direction,
                SideIndex = _turnTeller.CurrentTurn.SideIndex,
                StartingTileIndex = tileIndex
            });
        }

        private class BranchingLogic
        {
            private readonly Gameplay _gameplay;
            private readonly BoardStateView _boardStateView;
            private readonly IPlayTurnTeller _turnTeller;

            public BranchingLogic(Gameplay gameplay, BoardStateView boardStateView, IPlayTurnTeller turnTeller)
            {
                _gameplay = gameplay;
                _boardStateView = boardStateView;
                _turnTeller = turnTeller;
            }

            public void Branch()
            {
                CheckPiecesInMandarinTiles();
            }

            private void CheckPiecesInMandarinTiles()
            {
                if (AnyMandarinTileHasPieces())
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
                if (AnyTileOnCurrentSideHasPieces())
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
                if (AnyPiecesAvailableOnBenchOfCurrentSide())
                {
                    _gameplay.TakePiecesBackToBoard();
                }
                else
                {
                    _gameplay.GameOver();
                }
            }

            private bool AnyMandarinTileHasPieces()
            {
                return _boardStateView.AnyMandarinTileHasPieces;
            }

            private bool AnyTileOnCurrentSideHasPieces()
            {
                return _boardStateView.CheckAnyCitizenTileOnSideHasPieces(_turnTeller.CurrentTurn.SideIndex);
            }

            private bool AnyPiecesAvailableOnBenchOfCurrentSide()
            {
                return _boardStateView.CheckBenchOnSideHasPieces(_turnTeller.CurrentTurn.SideIndex);
            }
        }
    }
}