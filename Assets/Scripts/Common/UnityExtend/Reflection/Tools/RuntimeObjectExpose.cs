using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


namespace Common.UnityExtend.Reflection.Tools
{
    public class RuntimeObjectExpose
    {
        private IEnumerable<FieldInfo> _allFields;
        private IEnumerable<PropertyInfo> _allProperties;
        private readonly ITargetObjectProvider _objectProvider;

        public RuntimeObjectExpose(ITargetObjectProvider objectProvider)
        {
            _objectProvider = objectProvider;
        }

        public interface ITargetObjectProvider
        {
            object TargetObject { get; }
        }

        public IReadOnlyList<ObjectExposedItem> ExposeObject()
        {
            if (_objectProvider.TargetObject == null) return null;
            if (_allFields == null || _allProperties == null)
            {
                UpdateReflectionInfos();
            }

            var exposedItems = new List<ObjectExposedItem>();

            foreach (var fieldInfo in _allFields)
            {
                object value = null;
                try
                {
                    value = fieldInfo.GetValue(_objectProvider.TargetObject);
                }
                catch (Exception)
                {
                    //ignore
                }

                exposedItems.Add(new ObjectExposedItem
                {
                    FieldName = fieldInfo.Name,
                    DisplayValue = value?.ToString(),
                    IsPrimitive = IsPrimitive(fieldInfo.FieldType),
                    Value = value
                });
            }

            foreach (var propInfo in _allProperties)
            {
                object value = null;
                try
                {
                    value = propInfo.GetValue(_objectProvider.TargetObject);
                }
                catch (Exception)
                {
                    //ignore
                }

                exposedItems.Add(new ObjectExposedItem
                {
                    FieldName = propInfo.Name,
                    DisplayValue = value?.ToString(),
                    IsPrimitive = IsPrimitive(propInfo.PropertyType),
                    Value = value
                });
            }


            if (_objectProvider.TargetObject is Array arr)
            {
                for (var i = 0; i < arr.Length; i++)
                {
                    var value = arr.GetValue(i);
                    if (value == null) continue;
                    exposedItems.Add(new ObjectExposedItem
                    {
                        FieldName = $"Item {i}",
                        DisplayValue = value.ToString(),
                        IsPrimitive = IsPrimitive(value.GetType()),
                        Value = value
                    });
                }
            }

            return exposedItems;
        }

        public void UpdateReflectionInfos()
        {
            var type = _objectProvider.TargetObject.GetType();

            _allFields = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Instance); //ReflectionUtility.GetAllFields(type);
            _allProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Instance); //ReflectionUtility.GetAllProperties(type);
        }

        private static bool IsPrimitive(Type type)
        {
            return type.IsPrimitive || type.IsEnum || !type.IsClass;
        }

        public class ObjectExposedItem
        {
            public string FieldName;
            public string DisplayValue;
            public object Value;
            public bool IsPrimitive;
        }
#if UNITY_EDITOR

        public class CommonRuntimeObjectExposeEditor
        {
            private readonly Action<ObjectExposedItem> _clickEventHandler;
            private Vector2 _scrollPos;

            public CommonRuntimeObjectExposeEditor(Action<ObjectExposedItem> clickEventHandler)
            {
                _clickEventHandler = clickEventHandler;
            }

            public void DrawExposedItems(IEnumerable<ObjectExposedItem> exposedItems)
            {
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, EditorStyles.helpBox, GUILayout.ExpandWidth(true));
                EditorGUILayout.BeginVertical(GUI.skin.box);
                foreach (var exposedItem in exposedItems)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (!exposedItem.IsPrimitive && exposedItem.Value != null)
                    {
                        if (GUILayout.Button($"->", GUILayout.Width(25)))
                        {
                            _clickEventHandler?.Invoke(exposedItem);
                        }
                    }

                    EditorGUILayout.LabelField($"{exposedItem.FieldName}");
                    EditorGUILayout.LabelField($"{exposedItem.DisplayValue}");

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndScrollView();
            }
        }
#endif
    }
}