using Framework.DependencyInversion;
using Framework.Services;
using Gameplay.CoreGameplay.Controllers;
using Gameplay.Visual;
using Gameplay.Visual.Presenters;
using Gameplay.Visual.Views;

namespace System
{
    public class GameplayEventsHandler : DependencyInversionUnit
    {
        private BoardStatePresenter _boardStatePresenter;
        private BoardStateView _boardStateView;
        private ICoreGameplayController _controller;
        private BoardVisualView _boardVisualView;
        private BoardStateMatchVisualVerify _verify;
        private IMessageService _messageService;

        protected override void OnSetupDependencies()
        {
            base.OnSetupDependencies();
            _messageService = Resolver.Resolve<IMessageService>();
            _boardStatePresenter = Resolver.Resolve<BoardStatePresenter>();
            _boardStateView = Resolver.Resolve<BoardStateView>();
            _controller = Resolver.Resolve<ICoreGameplayController>();
            _boardVisualView = Resolver.Resolve<BoardVisualView>();
        }

        public void SetupForNewGame()
        {
            _verify = new BoardStateMatchVisualVerify(_boardStateView, _boardVisualView.BoardVisual);
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