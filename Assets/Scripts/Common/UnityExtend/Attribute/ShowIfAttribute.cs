using Common.UnityExtend.Reflection;
using Common.UnityExtend.Serialization;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Common.UnityExtend.Attribute
{
    public class ShowIfAttribute : PropertyAttribute
    {
        public ShowIfAttribute(string providerPropertyName, object value, string siblingCallbackName = "")
        {
            ProviderPropertyName = providerPropertyName;
            Value = value;
            SiblingCallbackName = siblingCallbackName;
            IsArray = false;
        }

        public ShowIfAttribute(string providerPropertyName, params object[] values)
        {
            ProviderPropertyName = providerPropertyName;
            ValueArray = values;
            SiblingCallbackName = "";
            IsArray = true;
        }

        public string SiblingCallbackName { get; }
        public string ProviderPropertyName { get; }
        public object Value { get; }
        public object[] ValueArray { get; }
        public bool IsArray { get; }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfDrawer : PropertyDrawer
    {
        private bool _prevToggled;
        private bool _toggled;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (attribute is not ShowIfAttribute att) return;

            if (_toggled)
            {
                EditorGUI.PropertyField(position, property, new GUIContent(label.text), true);
            }

            if (_prevToggled == _toggled) return;
            _prevToggled = _toggled;
            TryTriggerCallback(att, property);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            UpdateToggle(property);
            return _toggled ? EditorGUI.GetPropertyHeight(property, label, true) : 0;
        }

        private void TryTriggerCallback(ShowIfAttribute att, SerializedProperty property)
        {
            if (string.IsNullOrEmpty(att.SiblingCallbackName)) return;

            var parent = SerializeUtility.GetObjectToWhichPropertyBelong(property);
            var callback = parent.GetType().GetMethod(att.SiblingCallbackName, ReflectionUtility.MethodFlags);
            callback?.Invoke(parent, new object[] { _toggled });
        }

        private bool Equals(object probValue, ShowIfAttribute att)
        {
            if (att.IsArray)
            {
                return att.ValueArray.Any(o => o.Equals(probValue));
            }
            return probValue.Equals(att.Value);
        }

        private void UpdateToggle(SerializedProperty property)
        {
            if (attribute is not ShowIfAttribute att) return;
            var value = SerializeUtility.GetSiblingProperty(property, att.ProviderPropertyName);
            _toggled = Equals(value, att);// value.Equals(att.Value);
        }
    }
#endif
}