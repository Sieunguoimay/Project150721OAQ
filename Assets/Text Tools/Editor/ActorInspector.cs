// Copyright (C) 2019 Alexander Klochkov - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms


using UnityEditor;
using UnityEditorInternal;
using UnityEngine;


namespace texttools
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TextToolsActor))]
    public class ActorInspector : Editor
    {
        static private bool contentShow = false;
        static private bool ligthingShow = false;
        static private bool physicsShow = false;

        private SerializedProperty sourceFont;
        private SerializedProperty inputText;
        private SerializedProperty fontSize;
        private SerializedProperty characterSpace;
        private SerializedProperty wordSpace;
        private SerializedProperty lineSpace;
        private SerializedProperty verticalAlignment;
        private SerializedProperty horizontalAlignment;
        private SerializedProperty material;

        public void OnEnable()
        {
            sourceFont = serializedObject.FindProperty("sourceFont");
            inputText = serializedObject.FindProperty("inputText");
            fontSize = serializedObject.FindProperty("fontSize");
            characterSpace = serializedObject.FindProperty("characterSpace");
            wordSpace = serializedObject.FindProperty("wordSpace");
            lineSpace = serializedObject.FindProperty("lineSpace");
            verticalAlignment = serializedObject.FindProperty("verticalAlignment");
            horizontalAlignment = serializedObject.FindProperty("horizontalAlignment");
            material = serializedObject.FindProperty("material");
        }

        public override void OnInspectorGUI()
        {
            var generate = false;
            var clear = false;

            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(inputText, new GUIContent());
            EditorGUILayout.Space();

            if (EditorGUI.EndChangeCheck())
                generate = clear = true;

            if (contentShow = EditorGUILayout.Foldout(contentShow, "Content"))
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(sourceFont);
                EditorGUILayout.Space();

                if (EditorGUI.EndChangeCheck())
                    generate = clear = true;

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(verticalAlignment);
                EditorGUILayout.PropertyField(horizontalAlignment);
                EditorGUILayout.PropertyField(material);
                EditorGUILayout.PropertyField(fontSize);
                EditorGUILayout.PropertyField(characterSpace);
                EditorGUILayout.PropertyField(wordSpace);
                EditorGUILayout.PropertyField(lineSpace);

                if (EditorGUI.EndChangeCheck())
                    generate = true;
            }

            serializedObject.ApplyModifiedProperties();

            if (!generate) return;

            if (serializedObject.isEditingMultipleObjects)
            {
                foreach (var t in targets)
                {
                    (target as TextToolsActor)?.GenerateText(clear);
                }
            }
            else
            {
                (target as TextToolsActor)?.GenerateText(clear);
            }
        }
    }
}