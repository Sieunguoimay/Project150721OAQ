using System;
using Framework.Resolver;
using Gameplay.CoreGameplay.Controllers;
using Gameplay.CoreGameplay.Gateway;
using Gameplay.CoreGameplay.Interactors;
using Gameplay.CoreGameplay.Interactors.Simulation;
using Gameplay.Visual;
using Gameplay.Visual.Presenters;
using Gameplay.Visual.Views;
using UnityEngine;

namespace Gameplay.CoreGameplay
{
    [CreateAssetMenu]
    public partial class CoreGameplayLauncher : BaseGenericDependencyInversionScriptableObject<CoreGameplayLauncher>
    {
        private CoreGameplayController _controller;
        private CoreGameplayDataAccess _coreGameplayDataAccess;
        private SimulationResultPresenter _simulationResultPresenter;
        private PiecesMovingRunner _movingRunner;
        private MovingRunnerAdapter _adapter;

        protected override void OnBind(IBinder binder)
        {
            base.OnBind(binder);

            _controller = new CoreGameplayController();
            binder.Bind<ICoreGameplayController>(_controller);

            _coreGameplayDataAccess = new CoreGameplayDataAccess();
            _simulationResultPresenter = new SimulationResultPresenter();
            binder.Bind<ICoreGameplayDataAccess>(_coreGameplayDataAccess);
            binder.Bind<BoardStatePresenter>(new BoardStatePresenter());
            binder.Bind<BoardVisualPresenter>(new BoardVisualPresenter());
            binder.Bind<IBoardMoveSimulationResultHandler>(_simulationResultPresenter);

            _movingRunner = new PiecesMovingRunner();
            binder.Bind<PiecesMovingRunner>(_movingRunner);
        }

        protected override void OnUnbind(IBinder binder)
        {
            base.OnUnbind(binder);
            binder.Unbind<ICoreGameplayController>();
            binder.Unbind<ICoreGameplayDataAccess>();

            binder.Unbind<BoardStatePresenter>();
            binder.Unbind<BoardVisualPresenter>();
            binder.Unbind<IBoardMoveSimulationResultHandler>();
            binder.Unbind<PiecesMovingRunner>();
        }

        protected override void OnSetupDependencies()
        {
            base.OnSetupDependencies();

            _coreGameplayDataAccess.SetContainer(Resolver.Resolve<IGameplayContainer>());
            var installer = new CoreGameplayInstaller(_coreGameplayDataAccess, null, Resolver.Resolve<IBoardMoveSimulationResultHandler>());
            _controller.SetupDependencies(installer);
            _movingRunner.SetupDependencies(Resolver);

            _simulationResultPresenter.MovingRunner = _movingRunner;
            _adapter = new MovingRunnerAdapter(_movingRunner, _controller);

            _adapter.InstallMovingRunnerEvents();
        }

        protected override void OnTearDownDependencies()
        {
            base.OnTearDownDependencies();
            _adapter.UninstallMovingRunnerEvents();
        }
    }

    public class MovingRunnerAdapter
    {
        private readonly PiecesMovingRunner _movingRunner;
        private readonly ICoreGameplayController _controller;

        public MovingRunnerAdapter(PiecesMovingRunner movingRunner, ICoreGameplayController controller)
        {
            _movingRunner = movingRunner;
            _controller = controller;
        }

        public void InstallMovingRunnerEvents()
        {
            _movingRunner.AllMovingStepsExecutedEvent -= OnMovingRunnerEnded;
            _movingRunner.AllMovingStepsExecutedEvent += OnMovingRunnerEnded;
        }

        public void UninstallMovingRunnerEvents()
        {
            _movingRunner.AllMovingStepsExecutedEvent -= OnMovingRunnerEnded;
        }
        
        private void OnMovingRunnerEnded(PiecesMovingRunner obj)
        {
            _controller.NotifyGameplayOnSimulationPresentationEnded();
        }
    }
}