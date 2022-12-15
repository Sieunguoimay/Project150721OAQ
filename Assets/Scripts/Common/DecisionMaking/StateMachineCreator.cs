using System;
using System.Collections.Generic;
using System.Linq;
using Common.DecisionMaking.Actions;
using Common.UnityExtend.Attribute;
#if UNITY_EDITOR
using UnityEditor;
#endif
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
        private IEnumerable<StateTransition> StateTransitions => _stateTransitions ??= GetComponentsInChildren<StateTransition>();

        private void Start()
        {
            SetupTransitions();
            CreateFromScript();
        }

        public void CreateFromScript()
        {
            _stateMachine.SetStates(States.Select(s => s as IState).ToArray(), defaultStateIndex);
        }

        private void SetupTransitions()
        {
            foreach (var tr in StateTransitions)
            {
                tr.Setup(_stateMachine, GetIndex(tr.TargetState), GetIndex(tr.CurrentState));
            }
        }

        private int GetIndex(string stateName)
        {
            return States.Select((s, i) => (s, i)).FirstOrDefault(t => t.s.StateName.Equals(stateName)).i;
        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(StateMachineCreator))]
    public class StateMachineCreatorEditor : Editor
    {
        private bool _showStates;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            _showStates = EditorGUILayout.Foldout(_showStates, "States");
            if (_showStates)
            {
                var creator = target as StateMachineCreator;

                if (creator is null) return;
                foreach (var state in creator.States)
                {
                    EditorGUILayout.LabelField(state.StateName);
                }
            }
        }
    }
#endif
}