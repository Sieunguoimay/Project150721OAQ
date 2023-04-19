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

    public class PathSegment
    {
        public PathSegment(string memberName, Type reflectionType, bool valid)
        {
            MemberName = memberName;
            ReflectionType = reflectionType;
            Valid = valid;
        }

        public bool Valid { get; }
        public string MemberName { get; }
        public Type ReflectionType { get; }
    }

    public class Path
    {
        public IReadOnlyList<PathSegment> PathSegments { get; private set; }

        private Type _rootType;
        private string _path;

        public void UpdatePathSegments(Type rootType, string path)
        {
            if (_rootType == rootType) return;
            _rootType = rootType;
            UpdatePathSegments(path);
        }

        public void UpdatePathSegments(string path)
        {
            if (_path == path && PathSegments != null) return;
            if (_rootType == null) return;

            _path = path;

            var segments = string.IsNullOrEmpty(path) ? Array.Empty<string>() : path.Split('.');
            var pathSegments = new PathSegment[segments.Length];
            for (var i = 0; i < segments.Length; i++)
            {
                var valid = !string.IsNullOrEmpty(segments[i]) && ReflectionUtility.GetAllMembers(_rootType).Any(m => m.Name.Equals(ReflectionUtility.FormatName.Extract(segments[i])));
                pathSegments[i] = new PathSegment(segments[i], _rootType, valid);
                _rootType = ReflectionUtility.GetReturnTypeOfMember(_rootType, segments[i], true);
            }

            PathSegments = pathSegments;
        }
    }

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(PathSelectorAttribute))]
    public class PathSelectorDrawer : PropertyDrawer
    {
        private readonly Path _path = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (attribute is not PathSelectorAttribute attr) return;

            position = EditorGUI.PrefixLabel(position, label);

            if (property.propertyType == SerializedPropertyType.String)
            {
                DrawPathSelector(position, property, attr);
            }
        }

        private void DrawPathSelector(Rect position, SerializedProperty property, PathSelectorAttribute attr)
        {
            var pathSegments = GetPathSegments(property);
            var rootType = GetRootType(property, attr);

            _path.UpdatePathSegments(rootType, property.stringValue);

            var typeToShowMenu = DrawPathSegments(ref position, _path.PathSegments, out var segmentIndex);
            DrawPathSegmentModifyingButtons(position, property, pathSegments);
            if (segmentIndex != -1)
            {
                ShowMenu(property, typeToShowMenu, attr, segmentIndex, pathSegments);
            }
        }

        private static string[] GetPathSegments(SerializedProperty property)
        {
            return string.IsNullOrEmpty(property.stringValue) ? Array.Empty<string>() : property.stringValue.Split('.');
        }

        private static Type DrawPathSegments(ref Rect position, IReadOnlyList<PathSegment> pathSegments, out int pathSegmentIndexToOpenMenu)
        {
            position.width = 100;
            pathSegmentIndexToOpenMenu = -1;

            for (var i = 0; i < pathSegments.Count; i++)
            {
                var pathSegment = pathSegments[i];

                var guiContent = new GUIContent(ReflectionUtility.FormatName.Extract(pathSegment.MemberName), pathSegment.MemberName);

                var color = GUI.color;
                GUI.color = pathSegment.Valid ? color : Color.red;
                var openWindow = EditorGUI.DropdownButton(position, guiContent, FocusType.Keyboard);
                GUI.color = color;

                position.x += 102;

                if (openWindow && pathSegment.ReflectionType != null)
                {
                    pathSegmentIndexToOpenMenu = i;

                    return pathSegment.ReflectionType;
                }
            }

            return null;
        }

        private Type GetRootType(SerializedProperty property, PathSelectorAttribute pathSelector)
        {
            var siblingObject = GetSiblingObject(property, pathSelector);
            return pathSelector.IsTypeProvided ? siblingObject as Type : siblingObject?.GetType();
        }

        private static void ShowMenu(SerializedProperty property, Type rootType, PathSelectorAttribute objectSelector, int index, string[] pathSegments)
        {
            var menuItems = GetIds(rootType, objectSelector.IsGetPath);
            CreateMenu(property, menuItems, index, pathSegments).ShowAsContext();
        }

        private static GenericMenu CreateMenu(SerializedProperty property, IEnumerable<string> ids, int segmentIndex, string[] pathSegments)
        {
            var menu = new GenericMenu();

            foreach (var id in ids)
            {
                menu.AddItem(new GUIContent(id), property.stringValue == id, data => UpdatePathString(property, data, pathSegments, segmentIndex), id);
            }

            return menu;
        }

        private void DrawPathSegmentModifyingButtons(Rect position, SerializedProperty property, string[] pathSegments)
        {
            position.width = 25;

            if (pathSegments.Length > 0)
            {
                if (GUI.Button(position, new GUIContent("-", "Remove last path segment")))
                {
                    RemoveLastPathSegment(property, pathSegments);
                }

                position.x += 27;
            }

            if (GUI.Button(position, new GUIContent("+", "Add path segment")))
            {
                AddPathSegment(property);
            }
        }

        private void AddPathSegment(SerializedProperty property)
        {
            property.serializedObject.Update();
            property.stringValue = string.IsNullOrEmpty(property.stringValue)
                ? "null"
                : string.Concat(property.stringValue, ".");
            property.serializedObject.ApplyModifiedProperties();
            _path.UpdatePathSegments(property.stringValue);
        }

        private void RemoveLastPathSegment(SerializedProperty property, string[] pathSegments)
        {
            property.serializedObject.Update();
            property.stringValue = string.Join('.', pathSegments.Where((_, index) => index != pathSegments.Length - 1));
            property.serializedObject.ApplyModifiedProperties();
            _path.UpdatePathSegments(property.stringValue);
        }


        private static void UpdatePathString(SerializedProperty property, object data, string[] pathSegments, int segmentIndex)
        {
            pathSegments[segmentIndex] = ModifyValue((string)data);
            property.serializedObject.Update();
            property.stringValue = string.Join('.', pathSegments);
            property.serializedObject.ApplyModifiedProperties();
        }

        protected virtual object GetSiblingObject(SerializedProperty property,
            PathSelectorAttribute objectSelector)
        {
            return objectSelector.GetData(property);
        }

        private static IEnumerable<string> GetIds(Type type, bool isGetPath)
        {
            var allInterfaces = type.GetInterfaces();
            if (allInterfaces.Length > 0)
            {
                var interfaces = allInterfaces.Concat(new[] { type }).ToArray();
                var methods = interfaces.SelectMany(i =>
                    i.GetMethods().Where(WhereMethod)
                        .Select(mi => $"{i.Name}/{ReflectionUtility.FormatName.FormatMethodName(mi)}"));
                var props = interfaces.SelectMany(i =>
                    i.GetProperties().Select(pi => $"{i.Name}/{ReflectionUtility.FormatName.FormatPropertyName(pi)}"));
                var fields = interfaces.SelectMany(i =>
                    i.GetFields().Select(fi => $"{i.Name}/{ReflectionUtility.FormatName.FormatFieldName(fi)}"));
                return Array.Empty<string>().Concat(methods).Concat(props).Concat(fields);
            }
            else
            {
                var methods = type.GetMethods().Where(WhereMethod)
                    .Select(ReflectionUtility.FormatName.FormatMethodName);
                var props = type.GetProperties().Select(ReflectionUtility.FormatName.FormatPropertyName);
                var fields = type.GetFields().Select(ReflectionUtility.FormatName.FormatFieldName);
                return Array.Empty<string>().Concat(methods).Concat(props).Concat(fields);
            }

            bool WhereMethod(MethodInfo m)
            {
                var noParams = m.GetParameters().Length == 0;
                var hasReturnValue = m.ReturnType != typeof(void);
                var isGetProperty = m.Name.StartsWith("get_");
                if (isGetProperty) return false;
                if (!isGetPath) return true;
                return noParams && hasReturnValue;
            }
        }

        private static string ModifyValue(string value)
        {
            return value.Split('/').LastOrDefault();
        }
    }
#endif
}