using Framework.DependencyInversion;
using Gameplay.Visual.Views;

namespace Gameplay.CoreGameplay.Interactors.MoveDecisionMaking
{
    public class BoardActionDecisionMakingFactory :
        SelfBindingGenericDependencyInversionUnit<IBoardActionDecisionMakingFactory>,
        IBoardActionDecisionMakingFactory
    {
        private InteractSystem _interactSystem;

        protected override void OnSetupDependencies()
        {
            base.OnSetupDependencies();
            _interactSystem = Resolver.Resolve<InteractSystem>();
        }

        public IBoardActionDecisionMaking CreateDefaultDecisionMaking()
        {
            return new DefaultDecisionMaking();
        }

        public IBoardActionDecisionMaking CreatePlayerDecisionMaking()
        {
            return new PlayerDecisionMaking(_interactSystem);
        }

        public IBoardActionDecisionMaking CreateComputerDecisionMaking()
        {
            return new DefaultDecisionMaking();
        }
    }
}