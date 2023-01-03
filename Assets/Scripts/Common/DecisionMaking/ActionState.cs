using System;
using Common.Activity;
using Common.UnityExtend.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Common.DecisionMaking
{
    public class ActionState : MonoActivityQueue, IState
    {
        [field: SerializeField] public string StateName { get; private set; }
        [field: SerializeField] public UnityEvent Entered { get; private set; }
        [field: SerializeField] public UnityEvent Exited { get; private set; }

        public void Enter()
        {
            IsCurrent = true;
            Entered?.Invoke();
            Begin();
        }

        public void Exit()
        {
            IsCurrent = false;
            Exited?.Invoke();
            End();
        }

        [field: NonSerialized] public bool IsCurrent { get; private set; }

#if UNITY_EDITOR
        [ContextMenu("Use GameObject Name")]
        private void UseGameObjectName()
        {
            StateName = gameObject.name;
        }
#endif
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(ActionState))]
    public class ActionStateEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty(SerializeUtility.FormatBackingFieldPropertyName("StateName")));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(SerializeUtility.FormatBackingFieldPropertyName("Entered")));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(SerializeUtility.FormatBackingFieldPropertyName("Exited")));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("monoActivities"));
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}