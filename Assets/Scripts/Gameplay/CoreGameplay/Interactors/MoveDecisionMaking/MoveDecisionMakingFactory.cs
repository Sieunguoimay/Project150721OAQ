using Gameplay.Visual.Views;

namespace Gameplay.CoreGameplay.Interactors.MoveDecisionMaking
{
    public class MoveDecisionMakingFactory : IMoveDecisionMakingFactory
    {
        private readonly InteractSystem _interactSystem;

        public MoveDecisionMakingFactory(InteractSystem interactSystem)
        {
            _interactSystem = interactSystem;
        }

        public IMoveDecisionMaking CreateDefaultMoveDecisionMaking()
        {
            return new DefaultDecisionMaking();
        }

        public IMoveDecisionMaking CreatePlayerMoveDecisionMaking()
        {
            return new PlayerDecisionMaking(_interactSystem);
        }

        public IMoveDecisionMaking CreateComputerMoveDecisionMaking()
        {
            return new DefaultDecisionMaking();
        }
    }
}