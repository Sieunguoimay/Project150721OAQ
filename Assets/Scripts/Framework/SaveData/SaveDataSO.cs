using Sieunguoimay.Attribute;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace SaveData
{
    public class SaveDataSO : ScriptableObject
    {
#if UNITY_EDITOR
        [ContextMenuItem("Reset ID", nameof(GenerateID))]
#endif
        [SerializeField]
        private string id;

        public string GetSaveFileName() { return $"{name}-{id}"; }

#if UNITY_EDITOR
        [ContextMenu("Reset ID")]
#endif
        public void GenerateID()
        {
            var currentTimeStamp = DateTime.Now;
            var timeStampString = currentTimeStamp.ToString("yyyyMMddHHmmss");
            id = timeStampString;
        }

#if UNITY_EDITOR
        public void ResetAll()
        {
            var newInstance = ScriptableObject.CreateInstance(GetType()) as SaveDataSO;
            newInstance.name = name;
            newInstance.id = id;
            EditorUtility.CopySerialized(newInstance, this);
            UnityEngine.Object.DestroyImmediate(newInstance);
        }
#endif
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SaveDataSO),true)]
    public class UseGUIDAttributeDrawer : Editor
    {
        private SerializedProperty property;
        public override void OnInspectorGUI()
        {
            var enabled = GUI.enabled;
            GUI.enabled = false;
            DrawDefaultInspector();
            GUI.enabled = enabled;

            property ??= serializedObject.FindProperty("id");
            if (string.IsNullOrEmpty(property.stringValue))
            {
                (target as SaveDataSO)?.GenerateID();
            }
        }
    }
#endif
}