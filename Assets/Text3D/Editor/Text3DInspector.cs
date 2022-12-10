using UnityEditor;
using UnityEngine;

namespace Text3D.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Text3D.Scripts.Text3D))]
    public class Text3DInspector : UnityEditor.Editor
    {
        private static bool _contentShow;

        private SerializedProperty _sourceFont;
        private SerializedProperty _inputText;
        private SerializedProperty _fontSize;
        private SerializedProperty _characterSpace;
        private SerializedProperty _wordSpace;
        private SerializedProperty _lineSpace;
        private SerializedProperty _verticalAlignment;
        private SerializedProperty _horizontalAlignment;
        private SerializedProperty _material;

        public void OnEnable()
        {
            _sourceFont = serializedObject.FindProperty("sourceFont");
            _inputText = serializedObject.FindProperty("inputText");
            _fontSize = serializedObject.FindProperty("fontSize");
            _characterSpace = serializedObject.FindProperty("characterSpace");
            _wordSpace = serializedObject.FindProperty("wordSpace");
            _lineSpace = serializedObject.FindProperty("lineSpace");
            _verticalAlignment = serializedObject.FindProperty("verticalAlignment");
            _horizontalAlignment = serializedObject.FindProperty("horizontalAlignment");
            _material = serializedObject.FindProperty("material");
        }

        public override void OnInspectorGUI()
        {
            var generate = false;
            var clear = false;

            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_inputText, new GUIContent());
            EditorGUILayout.Space();

            if (EditorGUI.EndChangeCheck())
                generate = clear = true;

            if (_contentShow = EditorGUILayout.Foldout(_contentShow, "Content"))
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(_sourceFont);
                EditorGUILayout.Space();

                if (EditorGUI.EndChangeCheck())
                    generate = clear = true;

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(_verticalAlignment);
                EditorGUILayout.PropertyField(_horizontalAlignment);
                EditorGUILayout.PropertyField(_material);
                EditorGUILayout.PropertyField(_fontSize);
                EditorGUILayout.PropertyField(_characterSpace);
                EditorGUILayout.PropertyField(_wordSpace);
                EditorGUILayout.PropertyField(_lineSpace);

                if (EditorGUI.EndChangeCheck())
                    generate = true;
            }

            serializedObject.ApplyModifiedProperties();

            if (!generate) return;

            if (serializedObject.isEditingMultipleObjects)
            {
                foreach (var t in targets)
                {
                    (t as Text3D.Scripts.Text3D)?.GenerateText(clear);
                }
            }
            else
            {
                (target as Text3D.Scripts.Text3D)?.GenerateText(clear);
            }
        }
    }
}