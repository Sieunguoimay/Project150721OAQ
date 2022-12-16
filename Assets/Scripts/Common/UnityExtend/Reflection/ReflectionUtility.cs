using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Common.UnityExtend.Reflection
{
    public static class ReflectionUtility
    {
        public static object GetObjectAtPath(object obj, IEnumerable<string> path)
        {
            if (obj == null) return null;

            var finalObj = obj;
            foreach (var t in path)
            {
                finalObj = GetPropertyOrFieldValue(finalObj, t);
                if (finalObj == null) return null;
            }

            return finalObj;
        }

        public static Type GetTypeAtPath(Type obj, IEnumerable<string> path)
        {
            if (obj == null) return null;

            var finalObj = obj;
            foreach (var t in path)
            {
                finalObj = GetPropertyOrFieldType(finalObj, t);
                if (finalObj == null) return null;
            }

            return finalObj;
        }

        public static object GetSiblingProperty(SerializedProperty property, string name)
        {
            return GetPropertyOrFieldValue(GetParent(property), name);
        }

        public static object GetParent(SerializedProperty prop)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements.Take(elements.Length - 1))
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("[", StringComparison.Ordinal));
                    var index = Convert.ToInt32(element.Substring(element.IndexOf("[", StringComparison.Ordinal)).Replace("[", "").Replace("]", ""));
                    obj = GetValue(obj, elementName, index);
                }
                else
                {
                    obj = GetValue(obj, element);
                }
            }

            return obj;
        }

        private static object GetValue(object source, string name)
        {
            if (source == null)
                return null;
            var type = source.GetType();
            var f = type.GetField(name, FieldFlags);
            if (f == null)
            {
                var p = type.GetProperty(name, PropertyFlags);
                if (p == null)
                    return null;
                return p.GetValue(source, null);
            }

            return f.GetValue(source);
        }

        private static object GetValue(object source, string name, int index)
        {
            var enumerable = GetValue(source, name) as IEnumerable;
            var enm = enumerable.GetEnumerator();
            while (index-- >= 0)
                enm.MoveNext();
            return enm.Current;
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

        public static MethodInfo GetMethodInfo(Type type, string methodName)
        {
            return type.GetMethod(methodName,MethodFlags);
        }

        public static object GetPropertyOrFieldValue(object src, string propName)
        {
            var type = src.GetType();
            var prop = type.GetProperty(propName, PropertyFlags);
            var field = type.GetField(propName, FieldFlags);
            return prop == null ? field?.GetValue(src) : prop.GetValue(src, null);
        }

        public static Type GetPropertyOrFieldType(Type type, string propName)
        {
            var prop = type.GetProperty(propName, PropertyFlags);
            var field = type.GetField(propName, FieldFlags);
            return prop == null ? field?.FieldType : prop.PropertyType;
        }

        public static IEnumerable<Type> GetPropertyOrFieldTypes(Type type)
        {
            var prop = type.GetProperties(PropertyFlags);
            return prop.Length == 0 ? type.GetFields(FieldFlags)?.Select(f => f.FieldType) : prop.Select(f => f.PropertyType);
        }

        public static BindingFlags PropertyFlags => BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                                    BindingFlags.IgnoreCase;

        public static BindingFlags FieldFlags => BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

        public static BindingFlags MethodFlags => BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                                  BindingFlags.IgnoreCase;

        public static IEnumerable<Type> GetInterfaces(object obj)
        {
            if (obj is GameObject go)
            {
                return go.GetComponents<Component>().SelectMany(c => { return c.GetType().GetInterfaces().Concat(new[] {c.GetType()}); }).Concat(new[] {go.GetType()});
            }

            return obj.GetType().GetInterfaces().Concat(new[] {obj.GetType()});
        }
    }
}