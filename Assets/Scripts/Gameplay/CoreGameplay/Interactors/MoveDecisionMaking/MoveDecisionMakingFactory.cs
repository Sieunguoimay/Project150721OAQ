namespace Gameplay.CoreGameplay.Interactors.MoveDecisionMaking
{
    public class MoveDecisionMakingFactory : IMoveDecisionMakingFactory
    {
        public IMoveDecisionMaking CreateDefaultMoveDecisionMaking()
        {
            return new DefaultMoveDecisionMaking();
        }

        public IMoveDecisionMaking CreatePlayerMoveDecisionMaking()
        {
            return new DefaultMoveDecisionMaking();
        }

        public IMoveDecisionMaking CreateComputerMoveDecisionMaking()
        {
            return new DefaultMoveDecisionMaking();
        }
    }
}