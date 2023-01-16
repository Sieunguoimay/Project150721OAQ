using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;

namespace Framework.Entities.Variable.Boolean
{
    public class BooleanEntityManualView : VariableEntityManualView<bool>
    {
        [SerializeField] private UnityEvent onTrue;
        [SerializeField] private UnityEvent onFalse;

        protected override void OnValueChanged(object arg1, EventArgs arg2)
        {
            base.OnValueChanged(arg1, arg2);
            if (Entity.Value)
            {
                onTrue?.Invoke();
            }
            else
            {
                onFalse?.Invoke();
            }
        }
#if UNITY_EDITOR
        [SerializeField] private bool foldout;

        [CustomEditor(typeof(BooleanEntityManualView))]
        public class BooleanEntityManualViewEditor : Editor
        {
            private SerializedProperty _onTrue;
            private SerializedProperty _onFalse;
            private SerializedProperty _foldout;

            private void OnEnable()
            {
                _onTrue = serializedObject.FindProperty("onTrue");
                _onFalse = serializedObject.FindProperty("onFalse");
                _foldout = serializedObject.FindProperty("foldout");
            }

            public override void OnInspectorGUI()
            {
                serializedObject.Update();
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour(target as MonoBehaviour), typeof(BooleanEntityManualView), false);
                EditorGUI.EndDisabledGroup();

                _foldout.boolValue = EditorGUILayout.Foldout(_foldout.boolValue, "Events", true);
                if (_foldout.boolValue)
                {
                    EditorGUILayout.PropertyField(_onTrue);
                    EditorGUILayout.PropertyField(_onFalse);
                }

                serializedObject.ApplyModifiedProperties();
            }
        }
#endif
    }
}