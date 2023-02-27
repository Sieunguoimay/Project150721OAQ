﻿using Gameplay.Board;
using UnityEngine;

namespace Gameplay.DecisionMaking
{
    public interface IPlayerDecisionMaking
    {
        void MakeDecision();
    }

    public class PlayerDecisionMaking : IPlayerDecisionMaking
    {
        protected readonly BoardSide BoardSide;

        public PlayerDecisionMaking(BoardSide boardSide)
        {
            BoardSide = boardSide;
        }

        public void MakeDecision()
        {
            Debug.Log("Make Decision here..");
        }
    }
}