using Framework.Services;
using Gameplay.CoreGameplay.Controllers;
using Gameplay.Visual;
using Gameplay.Visual.Presenters;
using Gameplay.Visual.Views;

namespace System
{
    public class GameplayEventsHandler
    {
        private readonly BoardStatePresenter _boardStatePresenter;
        private readonly ICoreGameplayController _controller;
        private readonly BoardVisualView _boardVisualView;
        private readonly BoardStateMatchVisualVerify _verify;
        private readonly IMessageService _messageService;

        public GameplayEventsHandler(IMessageService messageService,
            BoardStatePresenter boardStatePresenter,
            ICoreGameplayController controller,
            BoardVisualView boardVisualView)
        {
            _messageService = messageService;
            _boardStatePresenter = boardStatePresenter;
            _controller = controller;
            _boardVisualView = boardVisualView;

            _verify = new BoardStateMatchVisualVerify(_boardStatePresenter.BoardStateView,
                _boardVisualView.BoardVisual);
            ConnectEvents();
        }

        public void Cleanup()
        {
            DisconnectEvents();
        }

        private void ConnectEvents()
        {
            _messageService.Register("AllMovingStepsExecutedEvent", OnAllMovingStepsDone);
            _boardVisualView.VisualReadyEvent -= OnBoardVisualReady;
            _boardVisualView.VisualReadyEvent += OnBoardVisualReady;
        }

        private void DisconnectEvents()
        {
            _boardVisualView.VisualReadyEvent -= OnBoardVisualReady;
            _messageService.Unregister("AllMovingStepsExecutedEvent", OnAllMovingStepsDone);
        }

        private void OnBoardVisualReady(BoardVisualView obj)
        {
            _controller.RunGameplay();
        }

        private void OnAllMovingStepsDone(object sender, EventArgs eventArgs)
        {
            _controller.RequestRefresh(_boardStatePresenter);

            _verify.Verify();

            _controller.CheckBranching();
        }
    }
}