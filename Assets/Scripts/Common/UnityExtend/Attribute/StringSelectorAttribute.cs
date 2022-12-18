using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common.UnityExtend.Reflection;
using UnityEditor;
using UnityEngine;

namespace Common.UnityExtend.Attribute
{
    public class BaseSelectorAttribute : PropertyAttribute
    {
        private readonly string _providerVariableName;
        private readonly bool _isProviderPropertyInBase;

        protected BaseSelectorAttribute(string name, bool isProviderPropertyInBase = false)
        {
            _providerVariableName = name;
            _isProviderPropertyInBase = isProviderPropertyInBase;
        }

        public object GetData(SerializedProperty property)
        {
            var providerObject = _isProviderPropertyInBase
                ? property.serializedObject.targetObject
                : ReflectionUtility.GetObjectToWhichPropertyBelong(property);
            return ReflectionUtility.GetDataFromMember(providerObject, _providerVariableName, false);
        }
    }

    public class StringSelectorAttribute : BaseSelectorAttribute
    {
        public StringSelectorAttribute(string name, bool isProviderPropertyInBase = false) : base(name,
            isProviderPropertyInBase)
        {
        }
    }

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(StringSelectorAttribute))]
    public class StringSelectorDrawer : PropertyDrawer
    {
        private GenericMenu _menu;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (attribute is not StringSelectorAttribute objectSelector) return;
            position = EditorGUI.PrefixLabel(position, label);

            if (property.propertyType == SerializedPropertyType.String)
            {
                if (!CreateMenuWithStringProperty(position, property, objectSelector)) return;
            }
            else if (property.propertyType == SerializedPropertyType.Integer)
            {
                if (!CreateMenuWithIntegerProperty(position, property, objectSelector)) return;
            }

            _menu?.ShowAsContext();
        }


        private bool CreateMenuWithIntegerProperty(Rect position, SerializedProperty property,
            StringSelectorAttribute objectSelector)
        {
            var openWindow =
                EditorGUI.DropdownButton(position, new GUIContent($"Index_{property.intValue}"),
                    FocusType.Keyboard);
            if (!openWindow)
            {
                _menu = null;
                return false;
            }

            if (_menu == null)
            {
                _menu = new GenericMenu();

                var ids = GetIds(property, objectSelector)?.ToArray();

                if (ids == null) return false;

                for (var i = 0; i < ids.Length; i++)
                {
                    var id = ids[i];
                    _menu.AddItem(new GUIContent(id), property.intValue == i, data =>
                    {
                        property.serializedObject.Update();
                        property.intValue = Array.IndexOf(ids, (string) data);
                        property.serializedObject.ApplyModifiedProperties();
                    }, id);
                }
            }

            return true;
        }

        private bool CreateMenuWithStringProperty(Rect position, SerializedProperty property,
            StringSelectorAttribute objectSelector)
        {
            var openWindow =
                EditorGUI.DropdownButton(position, new GUIContent(property.stringValue), FocusType.Keyboard);
            if (!openWindow)
            {
                _menu = null;
                return false;
            }

            if (_menu == null)
            {
                _menu = new GenericMenu();

                var ids = GetIds(property, objectSelector);

                if (ids == null) return false;

                foreach (var id in ids)
                {
                    _menu.AddItem(new GUIContent(id), property.stringValue == id, data =>
                    {
                        property.serializedObject.Update();
                        property.stringValue = (string) data;
                        property.serializedObject.ApplyModifiedProperties();
                    }, id);
                }
            }

            return true;
        }

        protected virtual IEnumerable<string> GetIds(SerializedProperty property,
            StringSelectorAttribute objectSelector)
        {
            return objectSelector.GetData(property) as IEnumerable<string>;
        }
    }
#endif
}