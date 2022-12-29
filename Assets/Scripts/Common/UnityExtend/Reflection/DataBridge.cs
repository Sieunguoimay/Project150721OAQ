﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common.UnityExtend.Attribute;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

namespace Common.UnityExtend.Reflection
{
    public class DataBridge : MonoBehaviour
    {
        [SerializeField, ComponentSelector] private Object sourceObject;

        [SerializeField, PathSelector(nameof(sourceObject))]
        private string path;

        [SerializeField] private EventList<SourceEventItem> eventItemList = new();

        [SerializeField] private MethodPairItem[] items;

        private object _runtimeSourceObject;

        private object GetSourceObject()
        {
            _runtimeSourceObject ??= ReflectionUtility.ExecutePathOfObject(sourceObject,
                string.IsNullOrEmpty(path) ? new string[0] : path.Split('.'), true);

            if (_runtimeSourceObject == null)
            {
                Debug.LogError($"Object at this path {sourceObject}->{path} is null");
            }

            return _runtimeSourceObject;
        }

        public Type SourceObjectType => ReflectionUtility.GetTypeAtPath(sourceObject?.GetType(),
            string.IsNullOrEmpty(path) ? new string[0] : path.Split('.'), true);

        [Serializable]
        public class MethodPairItem
        {
#if UNITY_EDITOR

            [PathSelector(nameof(SourceObjectType), true, true, true)]
#endif
            [SerializeField]
            private string sourceObjectMethodName;

            [SerializeField] private Object targetObject;

#if UNITY_EDITOR

            public IEnumerable<string> GetTargetMethodNames()
            {
                return targetObject.GetType()?
                    .GetMethods().Where(m => m.GetParameters().Length <= 1 && m.ReturnType == typeof(void))
                    .Select(ReflectionUtility.FormatName.FormatMethodName);
            }

            [StringSelector(nameof(GetTargetMethodNames))]
#endif
            [SerializeField]
            private string targetMethodName;

            [SerializeField] private FormatType formatType;

            private MethodInfo _targetMethodInfo;
            private Type _sourceMethodInfo;
            private int _numMethodParameters = 0;

            private UnityObjectPathSelector.PathExecutor _sourcePathExecutor = new();

            public void Transfer(object sourceObject)
            {
                SetupReflection(sourceObject);

                _targetMethodInfo.Invoke(targetObject, _numMethodParameters == 1
                    ? new[] {Format(_sourcePathExecutor.ExecutePath())}
                    : new object[0]);
            }

            public void SetupReflection(object sourceObject)
            {
                if (_targetMethodInfo != null && _sourceMethodInfo != null) return;

                _targetMethodInfo = ReflectionUtility.GetMethodInfo(targetObject.GetType(), targetMethodName, true);
                _sourceMethodInfo =
                    ReflectionUtility.GetTypeAtPath(sourceObject.GetType(), sourceObjectMethodName.Split('.'), true);

                _sourcePathExecutor.Setup(sourceObjectMethodName, sourceObject);

                if (_targetMethodInfo == null || _sourceMethodInfo == null)
                {
                    Debug.LogError("Methods not found");
                    return;
                }

                var t = _targetMethodInfo.GetParameters().FirstOrDefault()?.ParameterType;
                if (!t?.IsAssignableFrom(_sourceMethodInfo) ?? false)
                {
                    Debug.LogError(
                        $"Type mismatch  {_sourceMethodInfo} is not assignable to {_targetMethodInfo.GetParameters()[0].ParameterType}");
                }

                _numMethodParameters = _targetMethodInfo.GetParameters().Length;
            }

            public void Clear()
            {
                _targetMethodInfo = null;
                _sourceMethodInfo = null;
            }

            public object Format(object input)
            {
                return formatType switch
                {
                    FormatType.None => input,
                    FormatType.Float => input,
                    FormatType.Int => input,
                    FormatType.Double => input,
                    FormatType.Bool => input,
                    FormatType.NotBool => !(bool) input,
                    FormatType.String => input.ToString(),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        [Serializable]
        public enum FormatType
        {
            None,
            Float,
            Int,
            Double,
            Bool,
            NotBool,
            String
        }

        [Serializable]
        public class SourceEventItem : EventItem
        {
            public bool use;
        }

        private void OnValidate()
        {
            if (items != null)
            {
                foreach (var t in items)
                {
                    t.Clear();
                }
            }

            if (eventItemList != null)
            {
                var events = SourceObjectType?.GetEvents().Concat(GetExtraEvents()).ToArray();
                eventItemList.ValidateEventItems(events);
            }
        }

        private void OnEnable()
        {
            foreach (var t in items)
            {
                t.SetupReflection(GetSourceObject());
            }

            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private Delegate[] _cachedRuntimeDelegates;

        private void Subscribe()
        {
            var obj = GetSourceObject();
            _cachedRuntimeDelegates = new Delegate[eventItemList.EventItems.Length];
            for (var i = 0; i < eventItemList.EventItems.Length; i++)
            {
                var e = eventItemList.EventItems[i];
                if (!e.use) continue;

                var evInfo = e.GetEventInfo(obj.GetType());

                var runtimeDelegate = EventHandlerItem.CreateDelegate(evInfo.EventHandlerType, MethodInfo, this);
                if (runtimeDelegate == null) continue;

                evInfo.AddEventHandler(obj, runtimeDelegate);
                _cachedRuntimeDelegates[i] = runtimeDelegate;
            }
        }

        private void Unsubscribe()
        {
            var obj = GetSourceObject();

            for (var i = 0; i < eventItemList.EventItems.Length; i++)
            {
                var e = eventItemList.EventItems[i];
                if (!e.use) continue;

                var evInfo = e.GetEventInfo(obj.GetType());

                evInfo.RemoveEventHandler(obj, _cachedRuntimeDelegates[i]);
            }

            _cachedRuntimeDelegates = null;
        }

        private MethodInfo MethodInfo => GetType().GetMethod(nameof(OnEventTriggered), ReflectionUtility.MethodFlags);

        private void OnEventTriggered()
        {
            Transfer();
        }

        [ContextMenu("Transfer")]
        public void Transfer()
        {
            foreach (var t in items)
            {
                t.Transfer(GetSourceObject());
            }
        }


        #region ExtraEvents

        public event Action ThisEnabled;
        public event Action ThisDisabled;

        private IEnumerable<EventInfo> GetExtraEvents()
        {
            var type = GetType();
            return new[] {type.GetEvent(nameof(ThisEnabled)), type.GetEvent(nameof(ThisDisabled))};
        }

        #endregion
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(DataBridge.SourceEventItem))]
    public class SourceEventItemDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var eventName = property.FindPropertyRelative("eventName");
            var use = property.FindPropertyRelative("use");
            position.width -= 22;
            EditorGUI.LabelField(position, eventName.stringValue);
            position.x += position.width + 2;
            position.width = 20;
            use.boolValue = EditorGUI.Toggle(position, use.boolValue);
            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(DataBridge.MethodPairItem))]
    public class DataBridgeItemDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var sourceObjectMethodName = property.FindPropertyRelative("sourceObjectMethodName");
            var targetObject = property.FindPropertyRelative("targetObject");
            var targetMethodName = property.FindPropertyRelative("targetMethodName");
            var formatType = property.FindPropertyRelative("formatType");

            var fullWidth = position.width;
            var fullHeight = position.height;
            position.height = fullHeight / 2 - 2;
            position.width = fullWidth / 3 + 27;
            EditorGUI.PropertyField(position, sourceObjectMethodName, GUIContent.none);

            //Formatter
            const float w = 70;
            position.x += fullWidth - w + 1;
            position.width = w;
            EditorGUI.PropertyField(position, formatType, GUIContent.none);
            position.x -= fullWidth - w + 1;

            position.y += position.height;
            position.width = fullWidth / 3;
            EditorGUI.PropertyField(position, targetObject, GUIContent.none);

            var btnRect = position;
            btnRect.x += position.width + 2;
            btnRect.width = 25;
            TypeConstraintPropertyDrawer.Menu(btnRect, targetObject,
                go => go.GetComponents<Component>().Select(c => c as Object).Concat(new[] {go}).ToArray(), false);

            btnRect.x += 27;
            btnRect.width = fullWidth / 3 * 2 - 27;
            EditorGUI.PropertyField(btnRect, targetMethodName, GUIContent.none);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) * 2 + 4;
        }
    }
#endif
}