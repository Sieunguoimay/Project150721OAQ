using Framework.DependencyInversion;
using Framework.Resolver;
using Gameplay.CoreGameplay.Controllers;
using Gameplay.CoreGameplay.Gateway;
using Gameplay.CoreGameplay.Interactors;
using Gameplay.CoreGameplay.Interactors.MoveDecisionMaking;
using Gameplay.Visual;
using Gameplay.Visual.Presenters;
using Gameplay.Visual.Views;

namespace Gameplay.CoreGameplay
{
    public class CoreGameplayExternalInstaller : DependencyInversionScriptableObjectNode
    {
        protected override void OnBind(IBinder binder)
        {
            InstallExternalComponents();

            base.OnBind(binder);
        }

        private void InstallExternalComponents()
        {
            //Provide external components for internal components
            AddChildDependencyInversionUnit(new SimulationResultPresenter());
            AddChildDependencyInversionUnit(new ConcurrentSimulationResultPresenter());
            AddChildDependencyInversionUnit(new DecisionMakingFactory());
            AddChildDependencyInversionUnit(new CoreGameplayDataAccess());

            //Provide external components for external components
            AddChildDependencyInversionUnit(new SingleThreadPiecesMovingRunner());
            AddChildDependencyInversionUnit(new MultiThreadPiecesMovingRunner());
            AddChildDependencyInversionUnit(new BoardVisualPresenter());
            AddChildDependencyInversionUnit(new BoardStatePresenter());
        }
    }
}