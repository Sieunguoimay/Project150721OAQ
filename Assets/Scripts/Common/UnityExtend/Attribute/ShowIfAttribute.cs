using Common.UnityExtend.Reflection;
using UnityEditor;
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
        }

        public string SiblingCallbackName { get; }
        public string ProviderPropertyName { get; }
        public object Value { get; }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfDrawer : PropertyDrawer
    {
        private bool _toggled;
        private float _height = 16;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (attribute is not ShowIfAttribute att) return;
            var value = ReflectionUtility.GetSiblingProperty(property, att.ProviderPropertyName);

            var toggled = value.Equals(att.Value);

            // EditorGUI.BeginProperty(position, label, property);
            if (toggled)
            {
                var t = label.text;
                _height = EditorGUI.GetPropertyHeight(property, label, true);
                EditorGUI.PropertyField(position, property, new GUIContent(t), true);
            }

            // EditorGUI.EndProperty();

            if (_toggled == toggled) return;
            _toggled = toggled;

            if (string.IsNullOrEmpty(att.SiblingCallbackName)) return;

            var parent = ReflectionUtility.GetObjectToWhichPropertyBelong(property);
            var callback = parent.GetType().GetMethod(att.SiblingCallbackName, ReflectionUtility.MethodFlags);
            callback?.Invoke(parent, new object[] {_toggled});
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return _toggled ? _height : 0;
        }
    }
#endif
}