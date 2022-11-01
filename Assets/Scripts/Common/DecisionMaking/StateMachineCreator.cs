using System;
using System.Collections.Generic;
using System.Linq;
using Common.DecisionMaking.Actions;
using Common.UnityExtend.Attribute;
using UnityEngine;

namespace Common.DecisionMaking
{
    public class StateMachineCreator : MonoBehaviour
    {
        [SerializeField] private int defaultStateIndex = 0;

        private readonly StateMachine _stateMachine = new();

        private StateTransition[] _stateTransitions;
        private ActionState[] _states;

        public IEnumerable<ActionState> States => _states ??= GetComponentsInChildren<ActionState>();

        private void Start()
        {
            _stateTransitions = GetComponentsInChildren<StateTransition>();
            CreateFromScript();
            SetupTransitions();
        }

        public void CreateFromScript()
        {
            _stateMachine.SetStates(States.Select(s => s as IState).ToArray(), defaultStateIndex);
        }

        private void SetupTransitions()
        {
            foreach (var tr in _stateTransitions)
            {
                tr.Setup(_stateMachine, GetIndex(tr.TargetState));
            }
        }

        private int GetIndex(string stateName)
        {
            return States.Select((s, i) => (s, i)).FirstOrDefault(t => t.s.StateName.Equals(stateName)).i;
        }
    }
}