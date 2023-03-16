using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Common.UnityExtend.Reflection.Tools
{
    public class RuntimeObjectExpose
    {
        public IReadOnlyList<ObjectExposedItem> ExposeObject(object targetObject)
        {
            var type = targetObject.GetType();
            var allFields = ReflectionUtility.GetAllFields(type);
            var allProperties = ReflectionUtility.GetAllProperties(type);
            var exposedItems = new List<ObjectExposedItem>();

            foreach (var fieldInfo in allFields)
            {
                object value = null;
                try
                {
                    value = fieldInfo.GetValue(targetObject);
                }
                catch (Exception)
                {
                    //ignore
                }
                exposedItems.Add(new ObjectExposedItem
                {
                    FieldName = fieldInfo.Name,
                    DisplayValue = value?.ToString(),
                });
            }

            foreach (var propInfo in allProperties)
            {
                object value = null;
                try
                {
                    value = propInfo.GetValue(targetObject);
                }
                catch (Exception)
                {
                    //ignore
                }
                exposedItems.Add(new ObjectExposedItem
                {
                    FieldName = propInfo.Name,
                    DisplayValue = value?.ToString(),
                });
            }

            return exposedItems;
        }

        public class ObjectExposedItem
        {
            public string FieldName;
            public string DisplayValue;
        }
#if UNITY_EDITOR
        public static void DrawExposedItems(IEnumerable<ObjectExposedItem> exposedItems)
        {
            EditorGUILayout.BeginVertical();
            foreach (var exposedItem in exposedItems)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{exposedItem.FieldName}");
                EditorGUILayout.LabelField($"{exposedItem.DisplayValue}");
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }
#endif
    }
}