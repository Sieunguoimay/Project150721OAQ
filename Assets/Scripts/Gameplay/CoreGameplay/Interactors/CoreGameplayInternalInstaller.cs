using Framework.DependencyInversion;
using Framework.Resolver;
using Gameplay.CoreGameplay.Controllers;
using Gameplay.CoreGameplay.Interactors.OptionSystem;
using Gameplay.CoreGameplay.Interactors.Simulation;
using UnityEngine;

namespace Gameplay.CoreGameplay.Interactors
{
    public class CoreGameplayInternalInstaller : ScriptableEntity
    {
        [SerializeField] private SimulatorManager simulatorManager;
        private TurnDataExtractor _turnDataExtractor;
        private CoreGameplayController _coreGameplayController;

        protected override void OnBind(IBinder binder)
        {
            InstallInternalComponents();
            
            base.OnBind(binder);

            binder.Bind<BoardEntityAccess>(new BoardEntityAccess());
            
        }

        protected override void OnUnbind(IBinder binder)
        {
            binder.Unbind<BoardEntityAccess>();
            base.OnUnbind(binder);
        }

        private void InstallInternalComponents()
        {
            AddChildDependencyInversionUnit(new OptionSequenceFactory());
            AddChildDependencyInversionUnit(new RefreshRequester());
            AddChildDependencyInversionUnit(new CoreGameplayBranchingDriver());
            AddChildDependencyInversionUnit(simulatorManager);
            _turnDataExtractor = new TurnDataExtractor();
            _coreGameplayController = new CoreGameplayController();
            
            AddChildDependencyInversionUnit(_turnDataExtractor);
            AddChildDependencyInversionUnit(_coreGameplayController);
        }
    }
}