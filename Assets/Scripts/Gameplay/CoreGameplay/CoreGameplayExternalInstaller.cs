using Framework.DependencyInversion;
using Framework.Resolver;
using Gameplay.CoreGameplay.Gateway;
using Gameplay.CoreGameplay.Interactors.MoveDecisionMaking;
using Gameplay.Visual;
using Gameplay.Visual.Presenters;

namespace Gameplay.CoreGameplay
{
    public class CoreGameplayExternalInstaller : ScriptableEntity
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
            AddChildDependencyInversionUnit(new BoardStatePresenter());
        }
    }
}