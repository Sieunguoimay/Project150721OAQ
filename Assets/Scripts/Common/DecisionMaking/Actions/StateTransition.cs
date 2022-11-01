using System.Collections.Generic;
using System.Linq;
using Common.UnityExtend.Attribute;
using Gameplay.Piece.Activities;
using UnityEngine;

namespace Common.DecisionMaking.Actions
{
    public class StateTransition : MonoActivity
    {
        [field: SerializeField, StringSelector(nameof(StateNames))]
        public string TargetState { get; private set; }

        private StateMachine _stateMachine;
        private int _index;

        public void Setup(StateMachine stateMachine, int index)
        {
            _stateMachine = stateMachine;
            _index = index;
        }

        public void Transition()
        {
            _stateMachine.ChangeState(_index);
        }

        public override Activity CreateActivity()
        {
            return new ActivityCallback(Transition);
        }

#if UNITY_EDITOR
        private StateMachineCreator _stateMachineCreator;

        private IEnumerable<string> GetStates()
        {
            if (_stateMachineCreator == null)
            {
                _stateMachineCreator = GetComponentInParent<StateMachineCreator>();
            }

            return _stateMachineCreator.States.Select(s => s.StateName);
        }

        public IEnumerable<string> StateNames => GetStates();
#endif
    }
}