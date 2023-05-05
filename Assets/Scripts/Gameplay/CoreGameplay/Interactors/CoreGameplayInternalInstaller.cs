using Framework.DependencyInversion;
using Framework.Resolver;
using Gameplay.CoreGameplay.Controllers;
using Gameplay.CoreGameplay.Gateway;
using Gameplay.CoreGameplay.Interactors.MoveDecisionMaking;
using Gameplay.CoreGameplay.Interactors.OptionSystem;
using Gameplay.CoreGameplay.Interactors.Simulation;

namespace Gameplay.CoreGameplay.Interactors
{
    public class CoreGameplayInternalInstaller : DependencyInversionScriptableObjectNode
    {
        private TurnDataExtractor _turnDataExtractor;
        private CoreGameplayController _coreGameplayController;
        private readonly IContainer _coreGameplayContainer = new Container();

        protected override void OnBind(IBinder binder)
        {
            InstallInternalComponents();
            
            base.OnBind(_coreGameplayContainer);
            
            _coreGameplayContainer.Bind<BoardEntityAccess>(new BoardEntityAccess());
            
            binder.Bind<ICoreGameplayController>(_coreGameplayController);
            binder.Bind<TurnDataExtractor>(_turnDataExtractor);
        }

        protected override void OnUnbind(IBinder binder)
        {
            _coreGameplayContainer.Unbind<BoardEntityAccess>();
            
            binder.Unbind<ICoreGameplayController>();
            binder.Unbind<TurnDataExtractor>();
            
            base.OnUnbind(_coreGameplayContainer);
        }

        protected override void OnInject(IResolver resolver)
        {
            RebindExternalInterfaces(resolver);
            base.OnInject(_coreGameplayContainer);
        }

        private void RebindExternalInterfaces(IResolver resolver)
        {
            _coreGameplayContainer.Bind<IBoardMoveSimulationResultHandler>(
                resolver.Resolve<IBoardMoveSimulationResultHandler>());
            _coreGameplayContainer.Bind<IConcurrentMoveSimulationResultHandler>(
                resolver.Resolve<IConcurrentMoveSimulationResultHandler>());
            _coreGameplayContainer.Bind<IDecisionMakingFactory>(
                resolver.Resolve<IDecisionMakingFactory>());
            _coreGameplayContainer.Bind<ICoreGameplayDataAccess>(
                resolver.Resolve<ICoreGameplayDataAccess>());
        }

        private void InstallInternalComponents()
        {
            AddChildDependencyInversionUnit(new OptionSequenceFactory());
            AddChildDependencyInversionUnit(new RefreshRequester());
            AddChildDependencyInversionUnit(new CoreGameplayBranchingDriver());
            AddChildDependencyInversionUnit(new SimulatorFactory());
            AddChildDependencyInversionUnit(new DecisionMakingController());

            _turnDataExtractor = new TurnDataExtractor();
            _coreGameplayController = new CoreGameplayController();
            
            AddChildDependencyInversionUnit(_turnDataExtractor);
            AddChildDependencyInversionUnit(_coreGameplayController);
        }
    }
}