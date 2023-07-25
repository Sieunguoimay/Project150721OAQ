#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace SaveData
{
    public class SaveDataSO : ScriptableObject
    {
        [SerializeField, UseGUID]
        private string id;

        public string GetSaveFileName() { return $"{name}-{id}"; }

        public class UseGUIDAttribute : PropertyAttribute
        {
#if UNITY_EDITOR
            public void GenerateID(SerializedProperty property)
            {
                property.stringValue = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(property.serializedObject.targetObject));
            }
#endif
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SaveDataSO.UseGUIDAttribute))]
    public class UseGUIDAttributeDrawer : PropertyDrawer
    {
        private SerializedProperty _property;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _property = property;

            if (string.IsNullOrEmpty(_property.stringValue))
            {
                GenerateID();
            }
            var enabled = GUI.enabled;
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label);
            GUI.enabled = enabled;
        }

        [ContextMenu("Reset ID")]
        private void GenerateID()
        {
            (attribute as SaveDataSO.UseGUIDAttribute).GenerateID(_property);
        }
    }
#endif
}