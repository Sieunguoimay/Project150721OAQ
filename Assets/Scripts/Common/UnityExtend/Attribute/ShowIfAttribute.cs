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

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (attribute is not ShowIfAttribute att) return;
            var value = ReflectionUtility.GetSiblingProperty(property, att.ProviderPropertyName);

            var toggled = value.Equals(att.Value);

            if (toggled)
            {
                EditorGUI.PropertyField(position, property, label, true);
            }

            if (_toggled != toggled)
            {
                _toggled = toggled;
                if (!string.IsNullOrEmpty(att.SiblingCallbackName))
                {
                    var parent = ReflectionUtility.GetParent(property);
                    var callback = ReflectionUtility.GetMethodInfo(parent.GetType(), att.SiblingCallbackName);
                    callback?.Invoke(parent, new object[] {_toggled});
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return _toggled ? base.GetPropertyHeight(property, label) : 0;
        }
    }
#endif
}