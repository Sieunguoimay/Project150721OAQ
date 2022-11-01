﻿using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Common.UnityExtend.Attribute
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
    public class StringSelectorDrawer : PropertyDrawer
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

                var ids = GetIds(property, objectSelector);

                if (ids == null) return;

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

        protected virtual IEnumerable<string> GetIds(SerializedProperty property, StringSelectorAttribute objectSelector)
        {
            return ReflectionUtility.GetSiblingProperty(property, objectSelector.ProviderPropertyName) as IEnumerable<string>;
        }
    }
#endif
}