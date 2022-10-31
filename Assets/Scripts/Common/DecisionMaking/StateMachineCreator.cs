using System;
using System.Linq;
using UnityEngine;

namespace Common.DecisionMaking
{
    public class StateMachineCreator : MonoBehaviour
    {
        [SerializeField] private StateConfig[] stateConfigs;

        private readonly StateMachine _stateMachine = new();

        private StateTransition[] _stateTransitions;

        private void Start()
        {
            _stateTransitions = GetComponentsInChildren<StateTransition>();
            CreateFromScript();
            SetupTransitions();
        }

        public void CreateFromScript()
        {
            var states = new IState[stateConfigs.Length];
            var defaultStateIndex = stateConfigs.Select((s, index) => (s, index)).FirstOrDefault(t => t.s.isDefault).index;
            _stateMachine.SetStates(states, defaultStateIndex);
        }

        private void SetupTransitions()
        {
            foreach (var tr in _stateTransitions)
            {
                var index = GetIndex(tr.TargetState);
                tr.Setup(_stateMachine, index);
            }
        }

        private int GetIndex(string stateName)
        {
            return stateConfigs.Select((s, i) => (s, i)).FirstOrDefault(t => t.s.stateName.Equals(stateName)).i;
        }

        [Serializable]
        public class StateConfig
        {
            public string stateName;
            public bool isDefault;
        }
    }
}