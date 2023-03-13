using Gameplay.CoreGameplay.Controllers;
using Gameplay.CoreGameplay.Interactors.Simulation;
using Gameplay.GameInteract;
using Gameplay.PlayTurn;
using Gameplay.Visual;
using Gameplay.Visual.Presenters;
using Gameplay.Visual.Views;

namespace System
{
    public class GameplayViewActionCompleteHandler
    {
        private readonly BoardStatePresenter _boardStatePresenter;
        private readonly ICoreGameplayController _controller;
        private readonly PiecesMovingRunner _movingRunner;
        private readonly BoardVisualView _boardVisualView;
        private readonly BoardStateMatchVisualVerify _verify;

        public GameplayViewActionCompleteHandler(IGameplayContainer container,
            BoardStatePresenter boardStatePresenter,
            ICoreGameplayController controller,
            BoardVisualView boardVisualView)
        {
            _boardStatePresenter = boardStatePresenter;
            _controller = controller;
            _movingRunner = container.MovingRunner;
            _boardVisualView = boardVisualView;

            _verify = new BoardStateMatchVisualVerify(_boardStatePresenter.BoardStateView,
                _boardVisualView.BoardVisual);
            ConnectEvents();
        }

        public void Cleanup()
        {
            DisconnectEvents();
            _movingRunner.ResetMovingSteps();
        }

        private void ConnectEvents()
        {
            _movingRunner.AllMovingStepsExecutedEvent -= OnAllMovingStepsDone;
            _movingRunner.AllMovingStepsExecutedEvent += OnAllMovingStepsDone;

            _boardVisualView.VisualReadyEvent -= OnBoardVisualReady;
            _boardVisualView.VisualReadyEvent += OnBoardVisualReady;
        }

        private void DisconnectEvents()
        {
            _boardVisualView.VisualReadyEvent -= OnBoardVisualReady;
            _movingRunner.AllMovingStepsExecutedEvent -= OnAllMovingStepsDone;
        }

        private void OnBoardVisualReady(BoardVisualView obj)
        {
            _controller.RunGameplay();
        }

        private void OnAllMovingStepsDone(PiecesMovingRunner obj)
        {
            _controller.RequestRefresh(_boardStatePresenter);

            _verify.Verify();

            _controller.CheckBranching();
        }
    }
}