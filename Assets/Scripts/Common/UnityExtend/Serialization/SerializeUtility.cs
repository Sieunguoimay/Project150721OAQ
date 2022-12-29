using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.UnityExtend.Reflection;
using UnityEditor;

namespace Common.UnityExtend.Serialization
{
    public static class SerializeUtility
    {
        public static string FormatBackingFieldPropertyName(string name)
        {
            return $"<{name}>k__BackingField";
        }
        
        public static object GetSiblingProperty(SerializedProperty property, string name)
        {
            var src = GetObjectToWhichPropertyBelong(property);
            var type = src.GetType();
            var prop = ReflectionUtility. GetPropertyInfo(type, name, false);
            var field = ReflectionUtility.GetFieldInfo(type, name, false);
            return prop == null ? field?.GetValue(src) : prop.GetValue(src, null);
        }

        public static Type GetSiblingPropertyType(SerializedProperty property, string name)
        {
            var src = GetObjectToWhichPropertyBelong(property);
            var type = src.GetType();
            var prop = ReflectionUtility.GetPropertyInfo(type, name, false);
            var field = ReflectionUtility.GetFieldInfo(type, name, false);
            return prop == null ? field?.FieldType : prop.PropertyType;
        }

        public static object GetObjectToWhichPropertyBelong(SerializedProperty prop)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements.Take(elements.Length - 1))
            {
                if (element.Contains("["))
                {
                    var elementName = element[..element.IndexOf("[", StringComparison.Ordinal)];
                    var index = Convert.ToInt32(element[element.IndexOf("[", StringComparison.Ordinal)..]
                        .Replace("[", "").Replace("]", ""));
                    obj = ReflectionUtility.GetValueOfElement(ReflectionUtility.GetDataFromMember(obj, elementName, false) as IEnumerable, index);
                }
                else
                {
                    obj = ReflectionUtility.GetDataFromMember(obj, element, false);
                }
            }

            return obj;
        }
    }
}