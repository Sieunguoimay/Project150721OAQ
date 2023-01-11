using System;
using System.Collections.Generic;
using Common.UnityExtend.Attribute;
using Common.UnityExtend.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Framework.Services.Data
{
    public class DataAssetIdSelectorAttribute : StringSelectorAttribute
    {
        private readonly string _typeConstraintProvider;
        private readonly Type _typeConstraint;

        public DataAssetIdSelectorAttribute() : base("")
        {
        }

        public DataAssetIdSelectorAttribute(Type typeConstraint) : base("")
        {
            _typeConstraint = typeConstraint;
        }

        public DataAssetIdSelectorAttribute(string typeConstraintProvider) : base("")
        {
            _typeConstraintProvider = typeConstraintProvider;
        }

        private Type GetTypeConstraint(SerializedProperty property)
        {
            if (_typeConstraint != null) return _typeConstraint;
            if (!string.IsNullOrEmpty(_typeConstraintProvider))
            {
                return SerializeUtility.GetSiblingProperty(property, _typeConstraintProvider) as Type;
            }

            return null;
        }
#if UNITY_EDITOR

        [CustomPropertyDrawer(typeof(DataAssetIdSelectorAttribute))]
        private class IdSelectorDrawer : StringSelectorDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                var clickArea = position;
                Draw(position, property, new GUIContent(label.text, "Click Notify Asset"), new GUIStyle(GUI.skin.box) {alignment = TextAnchor.MiddleLeft});
                var current = Event.current;
                if (clickArea.Contains(current.mousePosition) && current.type == EventType.MouseDown && current.button == 0)
                {
                    if (!string.IsNullOrEmpty(property.stringValue))
                    {
                        EditorGUIUtility.PingObject(DataAssetIdHelper.GetDataAssetById(property.stringValue));
                    }

                    current.Use();
                }
            }

            protected override IEnumerable<string> GetIds(SerializedProperty property, StringSelectorAttribute objectSelector)
            {
                return DataAssetIdHelper.GetIds((objectSelector as DataAssetIdSelectorAttribute)?.GetTypeConstraint(property));
            }
        }
#endif
    }
}