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

        private StateTransition[] _stateTransitions;
        private ActionState[] _states;

        public IEnumerable<ActionState> States => _states ??= GetComponentsInChildren<ActionState>();
        private IEnumerable<StateTransition> StateTransitions => _stateTransitions ??= GetComponentsInChildren<StateTransition>();

        [field: System.NonSerialized] public StateMachine StateMachine { get; } = new();

        private void Awake()
        {
            SetupTransitions();
            CreateFromScript();
        }

        public void CreateFromScript()
        {
            StateMachine.SetStates(States.Select(s => s as IState).ToArray(), defaultStateIndex);
        }

        private void SetupTransitions()
        {
            foreach (var tr in StateTransitions)
            {
                tr.Setup(StateMachine, GetIndex(tr.TargetState), GetIndex(tr.CurrentState));
            }
        }

        public int GetIndex(string stateName)
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

            _showStates = EditorGUILayout.Foldout(_showStates, "States", true);

            if (!_showStates) return;
            if (target is not StateMachineCreator creator) return;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(10, false);
            EditorGUILayout.BeginVertical();
            foreach (var state in creator.States)
            {
                var isCurrent = Application.isPlaying && (ActionState) creator.StateMachine.CurrentState == state;
                EditorGUILayout.LabelField(state.StateName + (isCurrent ? "<-" : ""));
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
    }
#endif
}