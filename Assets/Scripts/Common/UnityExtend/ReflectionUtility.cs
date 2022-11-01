using System;
using System.Reflection;
using UnityEditor;

namespace Common.UnityExtend
{
    public static class ReflectionUtility
    {
        public static object GetSiblingProperty(SerializedProperty property, string name)
        {
            var pathToInterestedObject = property.propertyPath.Replace(property.name, "");
            if (pathToInterestedObject.Length > 0 && pathToInterestedObject[^1] == '.') pathToInterestedObject = pathToInterestedObject.Remove(pathToInterestedObject.Length - 1, 1);
            var sourceObject = string.IsNullOrEmpty(pathToInterestedObject) ? property.serializedObject.targetObject : GetPropertyValue(property.serializedObject.targetObject, pathToInterestedObject);
            return GetPropertyOrFieldValue(sourceObject, name);
        }

        public static object GetPropertyValue(object src, string propName)
        {
            while (true)
            {
                if (src == null) throw new ArgumentException("Value cannot be null.", nameof(src));
                if (propName == null) throw new ArgumentException("Value cannot be null.", nameof(propName));

                if (propName.Contains(".")) //complex type nested
                {
                    var temp = propName.Split(new[] {'.'}, 2);
                    src = GetPropertyValue(src, temp[0]);
                    propName = temp[1];
                }
                else
                {
                    return GetPropertyOrFieldValue(src, propName);
                }
            }
        }

        public static object GetPropertyOrFieldValue(object src, string propName)
        {
            var type = src.GetType();
            var prop = type.GetProperty(propName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            var field = type.GetField(propName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            return prop == null ? field?.GetValue(src) : prop.GetValue(src, null);
        }
    }
}