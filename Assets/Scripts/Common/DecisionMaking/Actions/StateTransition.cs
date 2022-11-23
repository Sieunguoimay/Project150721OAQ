﻿using System.Collections.Generic;
using System.Linq;
using Common.UnityExtend.Attribute;
using Gameplay.Piece.Activities;
using UnityEngine;

namespace Common.DecisionMaking.Actions
{
    public class StateTransition : MonoActivity
    {
        [SerializeField] private bool stateConstraint;
#if UNITY_EDITOR
        [field: StringSelector(nameof(StateNames))]
#endif
        [field: SerializeField, ShowIf(nameof(stateConstraint), true)]
        public string CurrentState { get; private set; }

#if UNITY_EDITOR
        [field: StringSelector(nameof(StateNames))]
#endif
        [field: SerializeField]
        public string TargetState { get; private set; }

        private StateMachine _stateMachine;
        private int _index;
        private int _constraintStateIndex;

        public void Setup(StateMachine stateMachine, int index, int constraintStateIndex)
        {
            _stateMachine = stateMachine;
            _index = index;
            _constraintStateIndex = constraintStateIndex;
        }

        public void Transition()
        {
            if (!stateConstraint || stateConstraint && _stateMachine.CurrentState == _constraintStateIndex)
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