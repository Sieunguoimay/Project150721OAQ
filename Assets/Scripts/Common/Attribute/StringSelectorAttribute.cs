using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Common.Attribute
{
    public class StringSelectorAttribute : PropertyAttribute
    {
        public StringSelectorAttribute(string name)
        {
            ProviderPropertyName = name;
        }

        public string ProviderPropertyName { get; }
    }

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(StringSelectorAttribute))]
    public class ObjectSelectorDrawer : PropertyDrawer
    {
        private GenericMenu _menu;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (attribute is not StringSelectorAttribute objectSelector) return;

            position = EditorGUI.PrefixLabel(position, label);
            var openWindow = EditorGUI.DropdownButton(position, new GUIContent(property.stringValue), FocusType.Keyboard);
            if (!openWindow)
            {
                _menu = null;
                return;
            }

            if (_menu == null)
            {
                _menu = new GenericMenu();
                var targetObjectType = property.serializedObject.targetObject.GetType();
                var propType = targetObjectType.GetProperty(objectSelector.ProviderPropertyName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                var fieldType = targetObjectType.GetField(objectSelector.ProviderPropertyName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                if (propType == null && fieldType == null) return;

                var ids = (propType.GetValue(property.serializedObject.targetObject) as string[]) ?? (fieldType.GetValue(property.serializedObject.targetObject) as string[]);
                if (ids == null)
                {
                    return;
                }

                foreach (var id in ids)
                {
                    _menu.AddItem(new GUIContent(id), property.stringValue == id, data =>
                    {
                        property.stringValue = (string) data;
                        property.serializedObject.ApplyModifiedProperties();
                    }, id);
                }
            }

            _menu.ShowAsContext();
        }
    }
#endif
}