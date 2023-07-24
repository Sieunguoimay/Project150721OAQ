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
        [SerializeField, ObjectBinderSO.Selector(typeof(BoardVisualGenerator))]
        private ObjectBinderSO boardVisualView;

        private BoardStatePresenter _boardStatePresenter;
        private BoardStateView _boardStateView;
        private ICoreGameplayController _controller;
        private BoardStateMatchVisualVerify _verify;
        private IMessageService _messageService;

        private BoardVisualGenerator BoardVisualGenerator => boardVisualView.GetRuntimeObject<BoardVisualGenerator>();
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
            _verify = new BoardStateMatchVisualVerify(_boardStateView, BoardVisualGenerator.BoardVisual);
            ConnectEvents();
        }

        public void Cleanup()
        {
            DisconnectEvents();
        }

        private void ConnectEvents()
        {
            _messageService.Register("AllMovingStepsExecutedEvent", OnAllMovingStepsDone);
            BoardVisualGenerator.VisualReadyEvent -= OnBoardVisualReady;
            BoardVisualGenerator.VisualReadyEvent += OnBoardVisualReady;
        }

        private void DisconnectEvents()
        {
            BoardVisualGenerator.VisualReadyEvent -= OnBoardVisualReady;
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