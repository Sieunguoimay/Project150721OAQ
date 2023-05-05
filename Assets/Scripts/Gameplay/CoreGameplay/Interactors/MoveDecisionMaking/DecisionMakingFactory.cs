using Framework.DependencyInversion;
using Gameplay.Visual.Views;

namespace Gameplay.CoreGameplay.Interactors.MoveDecisionMaking
{
    public class DecisionMakingFactory :
        SelfBindingGenericDependencyInversionUnit<IDecisionMakingFactory>,
        IDecisionMakingFactory
    {
        private InteractSystem _interactSystem;

        protected override void OnSetupDependencies()
        {
            base.OnSetupDependencies();
            _interactSystem = Resolver.Resolve<InteractSystem>();
        }

        public IDecisionMaker CreateDefaultDecisionMaking()
        {
            return new DefaultDecisionMaker();
        }

        public IDecisionMaker CreatePlayerDecisionMaking()
        {
            return new PlayerDecisionMaker(_interactSystem);
        }

        public IDecisionMaker CreateComputerDecisionMaking()
        {
            return new DefaultDecisionMaker();
        }
    }
}