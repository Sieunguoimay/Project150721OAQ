using Framework.DependencyInversion;
using Framework.Services;
using Gameplay.CoreGameplay.Controllers;
using Gameplay.Visual;
using Gameplay.Visual.Presenters;
using Gameplay.Visual.Views;
using UnityEngine;

namespace System
{
    public class GameplayEventsHandler : ScriptableEntity
    {
        [SerializeField] private BoardVisualGeneratorRepresenter boardVisualView;

        private BoardStatePresenter _boardStatePresenter;
        private BoardStateView _boardStateView;
        private ICoreGameplayController _controller;
        private BoardStateMatchVisualVerify _verify;
        private IMessageService _messageService;

        protected override void OnSetupDependencies()
        {
            base.OnSetupDependencies();
            _messageService = Resolver.Resolve<IMessageService>();
            _boardStatePresenter = Resolver.Resolve<BoardStatePresenter>();
            _boardStateView = Resolver.Resolve<BoardStateView>();
            _controller = Resolver.Resolve<ICoreGameplayController>();
        }

        public void SetupForNewGame()
        {
            _verify = new BoardStateMatchVisualVerify(_boardStateView, boardVisualView.Author.BoardVisual);
            ConnectEvents();
        }

        public void Cleanup()
        {
            DisconnectEvents();
        }

        private void ConnectEvents()
        {
            _messageService.Register("AllMovingStepsExecutedEvent", OnAllMovingStepsDone);
            boardVisualView.Author.VisualReadyEvent -= OnBoardVisualReady;
            boardVisualView.Author.VisualReadyEvent += OnBoardVisualReady;
        }

        private void DisconnectEvents()
        {
            boardVisualView.Author.VisualReadyEvent -= OnBoardVisualReady;
            _messageService.Unregister("AllMovingStepsExecutedEvent", OnAllMovingStepsDone);
        }

        private void OnBoardVisualReady(BoardVisualGenerator obj)
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