using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
#endif

namespace Sieunguoimay.Attribute
{
    public class DisableIfAttribute : PropertyAttribute
    {
        public readonly string PropertyName;
        public readonly bool Value;

        public DisableIfAttribute(string propertyName,bool value)
        {
            PropertyName = propertyName;
            Value = value;
        }
    }
#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(DisableIfAttribute))]
    public class DisableIfAttributeDrawer : PropertyDrawer
    {
        private bool _fistTime;
        private bool _shouldDisable;

        public DisableIfAttributeDrawer()
        {
            _fistTime = true;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_fistTime)
            {
                if (attribute is DisableIfAttribute att)
                {
                    var propName = att.PropertyName;
                    _shouldDisable = property.serializedObject.FindProperty(propName).boolValue == att.Value;
                }

                _fistTime = false;
            }

            EditorGUI.BeginProperty(position, label, property);
            var ge = GUI.enabled;
            GUI.enabled = !_shouldDisable;
            EditorGUI.PropertyField(position, property, label,true);
            GUI.enabled = ge;
            EditorGUI.EndProperty();
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property);
        }

    }
#endif
}