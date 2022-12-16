using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common.UnityExtend.Reflection;
using UnityEditor;
using UnityEngine;

namespace Common.UnityExtend.Attribute
{
    public class PathSelectorAttribute : PropertyAttribute
    {
        public PathSelectorAttribute(string objectPropertyName, bool isProviderPropertyInBase = false)
        {
            ObjectPropertyName = objectPropertyName;
            IsProviderPropertyInBase = isProviderPropertyInBase;
        }

        public string ObjectPropertyName { get; }
        public bool IsProviderPropertyInBase { get; }
    }

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(PathSelectorAttribute))]
    public class PathSelectorDrawer : PropertyDrawer
    {
        private GenericMenu _menu;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (attribute is not PathSelectorAttribute objectSelector) return;

            position.width = 50;
            EditorGUI.PrefixLabel(position, label);
            position.x = position.width;
            if (property.propertyType == SerializedPropertyType.String)
            {
                if (!CreateMenuWithStringProperty(position, property, objectSelector)) return;
            }
            else
            {
            }

            _menu?.ShowAsContext();
        }

        private bool CreateMenuWithStringProperty(Rect position, SerializedProperty property,
            PathSelectorAttribute objectSelector)
        {
            position.width = 100;

            var path = string.IsNullOrEmpty(property.stringValue) ? new string[0] : property.stringValue.Split('.');
            var rootType = GetSiblingObject(property, objectSelector).GetType();
            var open = false;
            for (var i = 0; i < path.Length; i++)
            {
                var p = path[i];

                if (i > 0)
                {
                    rootType = ReflectionUtility.GetPropertyOrFieldType(rootType, path[i - 1]);
                }

                var openWindow =
                    EditorGUI.DropdownButton(position, new GUIContent(p), FocusType.Keyboard);
                position.x += 102;

                if (!openWindow)
                {
                    continue;
                }

                if (rootType != null)
                {
                    _menu = new GenericMenu();


                    var ids = GetIds(rootType);

                    if (ids == null) continue;

                    foreach (var id in ids)
                    {
                        var i1 = i;
                        _menu.AddItem(new GUIContent(id), property.stringValue == id, data =>
                        {
                            path[i1] = (string) data;
                            property.stringValue = string.Join('.', path);
                            property.serializedObject.ApplyModifiedProperties();
                        }, id);
                    }

                    open = true;
                }
            }

            position.width = 25;

            if (path.Length > 0)
            {
                var remove = GUI.Button(position, "-");
                if (remove)
                {
                    property.stringValue = string.Join('.', path.Where((_, index) => index != path.Length - 1));
                    property.serializedObject.ApplyModifiedProperties();
                }

                position.x += 27;
            }

            var add = GUI.Button(position, "+");
            if (add)
            {
                property.stringValue = string.IsNullOrEmpty(property.stringValue) ? "null" : string.Concat(property.stringValue, ".");
                property.serializedObject.ApplyModifiedProperties();
            }

            return open;
        }

        protected virtual object GetSiblingObject(SerializedProperty property,
            PathSelectorAttribute objectSelector)
        {
            return objectSelector.IsProviderPropertyInBase
                ? ReflectionUtility.GetPropertyValue(property.serializedObject.targetObject,
                    objectSelector.ObjectPropertyName)
                : ReflectionUtility.GetSiblingProperty(property, objectSelector.ObjectPropertyName);
        }

        private static IEnumerable<string> GetIds(IReflect type)
        {
            return type.GetFields(ReflectionUtility.FieldFlags).Select(t => t.Name).Concat(type.GetProperties(ReflectionUtility.PropertyFlags).Select(p => p.Name));
        }
    }
#endif
}