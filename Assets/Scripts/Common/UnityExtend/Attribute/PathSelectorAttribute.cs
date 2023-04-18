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
        public bool IsTypeProvided { get; }

        public PathSelectorAttribute(string objectPropertyName, bool isGetPath = true,
            bool isProviderPropertyInBase = false, bool typeProvided = false)
            : base(objectPropertyName, isProviderPropertyInBase, "")
        {
            IsGetPath = isGetPath;
            IsTypeProvided = typeProvided;
        }
    }

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(PathSelectorAttribute))]
    public class PathSelectorDrawer : PropertyDrawer
    {
        private bool _changed;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (attribute is not PathSelectorAttribute objectSelector) return;

            position = EditorGUI.PrefixLabel(position, label);
            if (property.propertyType == SerializedPropertyType.String)
            {
                CreateMenuWithStringProperty(position, property, objectSelector)?.ShowAsContext();
            }
        }

        private GenericMenu CreateMenuWithStringProperty(Rect position, SerializedProperty property,
            PathSelectorAttribute objectSelector)
        {
            position.width = 100;

            var pathSegments = string.IsNullOrEmpty(property.stringValue) ? Array.Empty<string>() : property.stringValue.Split('.');
            var siblingObject = GetSiblingObject(property, objectSelector);
            var rootType = objectSelector.IsTypeProvided ? siblingObject as Type : siblingObject?.GetType();
            if (rootType == null) return null;

            GenericMenu menu = null;
            for (var i = 0; i < pathSegments.Length; i++)
            {
                var pathSegment = pathSegments[i];

                if (i > 0)
                {
                    rootType = ReflectionUtility.GetReturnTypeOfMember(rootType, pathSegments[i - 1], true);
                }

                var guiContent = new GUIContent(ReflectionUtility.FormatName.Extract(pathSegment), pathSegment);
                var openWindow = EditorGUI.DropdownButton(position, guiContent, FocusType.Keyboard);
                
                position.x += 102;

                if (openWindow && rootType != null)
                {
                    var menuItems = GetIds(rootType, objectSelector.IsGetPath);
                    menu = CreateMenu(property, menuItems, i, pathSegments);
                }
            }

            if (_changed)
            {
                _changed = false;
                GUI.changed = true;
            }

            position.width = 25;

            if (pathSegments.Length > 0)
            {
                if (GUI.Button(position, new GUIContent("-","Remove last path segment")))
                {
                    RemoveLastPathSegment(property, pathSegments);
                }

                position.x += 27;
            }

            if (GUI.Button(position, new GUIContent("+","Add path segment")))
            {
                AddPathSegment(property);
            }

            return menu;
        }

        private static void AddPathSegment(SerializedProperty property)
        {
            property.serializedObject.Update();
            property.stringValue = string.IsNullOrEmpty(property.stringValue)
                ? "null"
                : string.Concat(property.stringValue, ".");
            property.serializedObject.ApplyModifiedProperties();
        }

        private static void RemoveLastPathSegment(SerializedProperty property, string[] pathSegments)
        {
            property.serializedObject.Update();
            property.stringValue = string.Join('.', pathSegments.Where((_, index) => index != pathSegments.Length - 1));
            property.serializedObject.ApplyModifiedProperties();
        }

        private GenericMenu CreateMenu(SerializedProperty property, IEnumerable<string> ids, int segmentIndex, string[] pathSegments)
        {
            var menu = new GenericMenu();

            foreach (var id in ids)
            {
                menu.AddItem(new GUIContent(id), property.stringValue == id, data => OnSelected(property, data, pathSegments, segmentIndex), id);
            }

            return menu;
        }

        private void OnSelected(SerializedProperty property, object data, string[] pathSegments, int segmentIndex)
        {
            pathSegments[segmentIndex] = ModifyValue((string)data);
            property.serializedObject.Update();
            property.stringValue = string.Join('.', pathSegments);
            property.serializedObject.ApplyModifiedProperties();
            _changed = true;
        }

        protected virtual object GetSiblingObject(SerializedProperty property,
            PathSelectorAttribute objectSelector)
        {
            return objectSelector.GetData(property);
        }

        private static IEnumerable<string> GetIds(Type type, bool isGetPath)
        {
            // var result = ReflectionUtility.GetAllMethodsAndInterfaces(type);
            // return result.SelectMany(r => r.Item2.Where(m => m.ReturnType == typeof(void)).Select(mi => $"{r.Item1.Name}/{ReflectionUtility.FormatName.FormatMethodName(mi)}"));
            var types = type.GetInterfaces();
            if (types.Length > 0)
            {
                var interfaces = types.Concat(new[] { type });
                var methods = interfaces.SelectMany(i =>
                    i.GetMethods().Where(WhereMethod)
                        .Select(mi => $"{i.Name}/{ReflectionUtility.FormatName.FormatMethodName(mi)}"));
                var props = interfaces.SelectMany(i =>
                    i.GetProperties().Select(pi => $"{i.Name}/{ReflectionUtility.FormatName.FormatPropertyName(pi)}"));
                var fields = interfaces.SelectMany(i =>
                    i.GetFields().Select(fi => $"{i.Name}/{ReflectionUtility.FormatName.FormatFieldName(fi)}"));
                return new string[0].Concat(methods).Concat(props).Concat(fields);
            }
            else
            {
                var methods = type.GetMethods().Where(WhereMethod)
                    .Select(ReflectionUtility.FormatName.FormatMethodName);
                var props = type.GetProperties().Select(ReflectionUtility.FormatName.FormatPropertyName);
                var fields = type.GetFields().Select(ReflectionUtility.FormatName.FormatFieldName);
                return new string[0].Concat(methods).Concat(props).Concat(fields);
            }

            bool WhereMethod(MethodInfo m)
            {
                var hasParams = m.GetParameters().Length == 0;
                var hasReturnValue = m.ReturnType != typeof(void);
                var isGetProperty = m.Name.StartsWith("get_");
                return !isGetProperty && (!isGetPath || !hasParams && hasReturnValue);
            }
        }

        private static string ModifyValue(string value)
        {
            return value.Split('/').LastOrDefault();
        }
    }
#endif
}