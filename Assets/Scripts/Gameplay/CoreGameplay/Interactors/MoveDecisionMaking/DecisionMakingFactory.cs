using Framework.DependencyInversion;
using Gameplay.Visual.Views;
using System;
using UnityEngine;

namespace Gameplay.CoreGameplay.Interactors.MoveDecisionMaking
{
    public class DecisionMakingFactory : ScriptableEntity, IDecisionMakingFactory
    {
        [SerializeField] private InteractSystemRepresenter interactSystem;
        protected override Type GetBindingType()
        {
            return typeof(IDecisionMakingFactory);
        }

        public IDecisionMaker CreateDefaultDecisionMaking()
        {
            return new DefaultDecisionMaker();
        }

        public IDecisionMaker CreatePlayerDecisionMaking()
        {
            return new PlayerDecisionMaker(interactSystem.Author);
        }

        public IDecisionMaker CreateComputerDecisionMaking()
        {
            return new DefaultDecisionMaker();
        }
    }
}