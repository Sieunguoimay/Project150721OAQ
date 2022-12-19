using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common.UnityExtend.Reflection;
using UnityEditor;
using UnityEngine;

namespace Common.UnityExtend.Attribute
{
    public class PathSelectorAttribute : BaseSelectorAttribute
    {
        public bool IsGetPath { get; }

        public PathSelectorAttribute(string objectPropertyName, bool isGetPath = true,
            bool isProviderPropertyInBase = false) : base(
            objectPropertyName, isProviderPropertyInBase)
        {
            IsGetPath = isGetPath;
        }
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

            _menu?.ShowAsContext();
        }

        private bool _changed;

        private bool CreateMenuWithStringProperty(Rect position, SerializedProperty property,
            PathSelectorAttribute objectSelector)
        {
            position.width = 100;

            var path = string.IsNullOrEmpty(property.stringValue) ? new string[0] : property.stringValue.Split('.');
            var rootType = GetSiblingObject(property, objectSelector)?.GetType();
            if (rootType == null) return false;
            var open = false;
            for (var i = 0; i < path.Length; i++)
            {
                var p = path[i];

                if (i > 0)
                {
                    rootType = ReflectionUtility.GetReturnTypeOfMember(rootType, path[i - 1], true);
                }

                var openWindow = EditorGUI.DropdownButton(position,
                    new GUIContent(ReflectionUtility.FormatName.Extract(p)), FocusType.Keyboard);
                position.x += 102;

                if (!openWindow) continue;
                if (rootType == null) continue;

                _menu = new GenericMenu();

                var ids = GetIds(rootType, objectSelector.IsGetPath);

                if (ids == null) continue;

                foreach (var id in ids)
                {
                    var i1 = i;
                    _menu.AddItem(new GUIContent(id), property.stringValue == id, data =>
                    {
                        path[i1] = (string) data;
                        property.serializedObject.Update();
                        property.stringValue = string.Join('.', path);
                        property.serializedObject.ApplyModifiedProperties();
                        _changed = true;
                    }, id);
                }

                open = true;
            }

            if (_changed)
            {
                _changed = false;
                GUI.changed = true;
            }

            position.width = 25;

            if (path.Length > 0)
            {
                var remove = GUI.Button(position, "-");
                if (remove)
                {
                    property.serializedObject.Update();
                    property.stringValue = string.Join('.', path.Where((_, index) => index != path.Length - 1));
                    property.serializedObject.ApplyModifiedProperties();
                }

                position.x += 27;
            }

            var add = GUI.Button(position, "+");
            if (add)
            {
                property.serializedObject.Update();
                property.stringValue = string.IsNullOrEmpty(property.stringValue)
                    ? "null"
                    : string.Concat(property.stringValue, ".");
                property.serializedObject.ApplyModifiedProperties();
            }

            return open;
        }

        protected virtual object GetSiblingObject(SerializedProperty property,
            PathSelectorAttribute objectSelector)
        {
            return objectSelector.GetData(property);
        }

        private static IEnumerable<string> GetIds(Type type, bool isGetPath)
        {
            var methods = ReflectionUtility.GetAllMethods(type);
            var props = ReflectionUtility.GetAllProperties(type);
            var fields = ReflectionUtility.GetAllFields(type);
            return new string[0]
                    .Concat(methods.Where(m => !m.Name.StartsWith("get_")).Where(m =>
                            !isGetPath || m.GetParameters().Length == 0 && m.ReturnType != typeof(void))
                        .Select(ReflectionUtility.FormatName.FormatMethodName))
                    .Concat(props.Select(ReflectionUtility.FormatName.FormatPropertyName))
                    .Concat(fields.Select(ReflectionUtility.FormatName.FormatFieldName))
                ;
        }
    }
#endif
}