using Gameplay.Visual.Board;
using UnityEngine;

namespace Gameplay.DecisionMaking
{
    public interface IPlayerDecisionMaking
    {
        void MakeDecision();
    }

    public class PlayerDecisionMaking : IPlayerDecisionMaking
    {
        protected readonly BoardSideVisual BoardSide;

        public PlayerDecisionMaking(BoardSideVisual boardSide)
        {
            BoardSide = boardSide;
        }

        public void MakeDecision()
        {
            Debug.Log("Make Decision here..");
        }
    }
}