using Gameplay.CoreGameplay.Controllers;

namespace Gameplay.CoreGameplay.Interactors.DecisionMaking
{
    public class DecisionMakingFactory : IDecisionMakingFactory
    {
        public IDecisionMaking CreateDefaultDecisionMaking()
        {
            return new DefaultDecisionMaking();
        }

        public IDecisionMaking CreatePlayerDecisionMaking()
        {
            return new DefaultDecisionMaking();
        }

        public IDecisionMaking CreateComputerDecisionMaking()
        {
            return new DefaultDecisionMaking();
        }
    }
}