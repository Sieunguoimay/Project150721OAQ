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
            return new DefaultMoveDecisionMaking();
        }

        public IMoveDecisionMaking CreatePlayerMoveDecisionMaking()
        {
            return new PlayerMoveDecisionMaking(_interactSystem);
        }

        public IMoveDecisionMaking CreateComputerMoveDecisionMaking()
        {
            return new DefaultMoveDecisionMaking();
        }
    }
}