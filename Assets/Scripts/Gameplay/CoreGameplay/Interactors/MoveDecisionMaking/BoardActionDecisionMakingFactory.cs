using Gameplay.Visual.Views;

namespace Gameplay.CoreGameplay.Interactors.MoveDecisionMaking
{
    public class BoardActionDecisionMakingFactory : IBoardActionDecisionMakingFactory
    {
        private readonly InteractSystem _interactSystem;

        public BoardActionDecisionMakingFactory(InteractSystem interactSystem)
        {
            _interactSystem = interactSystem;
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