using UnityEditor;
using UnityEngine;

namespace Common.UnityExtend.Attribute
{
    public class ShowIfAttribute : PropertyAttribute
    {
        public ShowIfAttribute(string providerPropertyName, object value)
        {
            ProviderPropertyName = providerPropertyName;
            Value = value;
        }

        public string ProviderPropertyName { get; }
        public object Value { get; }
    }
    
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfDrawer : PropertyDrawer
    {
        private bool _toggled;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (attribute is not ShowIfAttribute att) return;
            var value = ReflectionUtility.GetSiblingProperty(property, att.ProviderPropertyName);
            _toggled = value.Equals(att.Value);
            
            if (_toggled) EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return _toggled ? base.GetPropertyHeight(property, label) : 0;
        }
    }
#endif
}