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
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(BooleanEntityManualView))]
    public class BooleanEntityManualViewEditor : Editor
    {
        private bool _foldout;
        private SerializedProperty _onTrue;
        private SerializedProperty _onFalse;

        private void OnEnable()
        {
            _onTrue = serializedObject.FindProperty("onTrue");
            _onFalse = serializedObject.FindProperty("onFalse");
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour(target as MonoBehaviour), typeof(BooleanEntityManualView), false);
            EditorGUI.EndDisabledGroup();

            _foldout = EditorGUILayout.Foldout(_foldout, "Events", true);
            if (_foldout)
            {
                EditorGUILayout.PropertyField(_onFalse);
                EditorGUILayout.PropertyField(_onTrue);
            }
        }
    }
#endif
}