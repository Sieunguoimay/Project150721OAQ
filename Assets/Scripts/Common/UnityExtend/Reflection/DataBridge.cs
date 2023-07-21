using System;
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
    public interface IDataBridge
    {
        void Transfer();
    }

    public class DataBridge : MonoBehaviour, IDataBridge
    {
        [SerializeField] private UnityObjectPathSelector pathSelector = new();

        [SerializeField] private EventList<SourceEventItem> eventItemList = new();

        [SerializeField] private MethodPairItem[] items;

        public Type SourceObjectType => PathSelector.PathFinalType;

        private UnityObjectPathSelector PathSelector
        {
            get
            {
                if (pathSelector.Executor == null)
                {
                    pathSelector.Setup(true);
                }

                return pathSelector;
            }
        }

        [Serializable]
        public class MethodPairItem
        {
#if UNITY_EDITOR

            [PathSelector(nameof(SourceObjectType), true, true)]
#endif
            [SerializeField]
            private string sourceObjectMethodName;

            [SerializeField, UnityObjectPathSelector.Compact]
            private UnityObjectPathSelector targetObject;

#if UNITY_EDITOR

            public IEnumerable<string> GetTargetMethodNames()
            {
                return targetObject.PathFinalType?
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

            private readonly UnityObjectPathSelector.PathExecutor _sourcePathExecutor = new();

            public void Transfer(object sourceObject)
            {
                try
                {
                    SetupReflection(sourceObject);
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}. {sourceObject.GetType()}");
                }

                _targetMethodInfo?.Invoke(targetObject.Executor.CachedRuntimeObject, _numMethodParameters == 1
                    ? new[] { Format(_sourcePathExecutor.ExecutePath()) }
                    : Array.Empty<object>());
            }

            public void SetupReflection(object sourceObject)
            {
                if (sourceObject == null) return;
                if (_targetMethodInfo != null && _sourceMethodInfo != null) return;
                targetObject.Setup(true);

                _targetMethodInfo = ReflectionUtility.GetMethodInfo(targetObject.PathFinalType, targetMethodName, true);
                _sourceMethodInfo =
                    ReflectionUtility.GetTypeAtPath(sourceObject.GetType(), sourceObjectMethodName.Split('.'), true);

                _sourcePathExecutor.Setup(sourceObjectMethodName, sourceObject, false);
                if (_targetMethodInfo == null || _sourceMethodInfo == null)
                {
                    Debug.LogError($"Methods not found {this}");
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
                    FormatType.NotBool => !(bool)input,
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
                var events = ReflectionUtility.GetAllEvents(SourceObjectType).Concat(GetExtraEvents()).ToArray();
                eventItemList.ValidateEventItems(events);
            }
        }

        private void OnEnable()
        {
            foreach (var t in items)
            {
                t.SetupReflection(PathSelector.Executor.CachedRuntimeObject);
            }

            Subscribe();
            ThisEnabled?.Invoke();
        }

        private void OnDisable()
        {
            Unsubscribe();
            ThisDisabled?.Invoke();
        }

        private Delegate[] _cachedRuntimeDelegates;

        private void Subscribe()
        {
            var obj = PathSelector.Executor.CachedRuntimeObject;
            _cachedRuntimeDelegates = new Delegate[eventItemList.EventItems.Length];
            for (var i = 0; i < eventItemList.EventItems.Length; i++)
            {
                var e = eventItemList.EventItems[i];
                if (!e.use) continue;
                var o = IsExtraEventItem(e) ? this : obj;
                if (o == null) continue;
                var evInfo = e.GetEventInfo(o.GetType());

                var runtimeDelegate = EventHandlerItem.CreateDelegate(evInfo.EventHandlerType, MethodInfo, this);
                if (runtimeDelegate == null) continue;

                evInfo.AddEventHandler(o, runtimeDelegate);
                _cachedRuntimeDelegates[i] = runtimeDelegate;
            }
        }

        private void Unsubscribe()
        {
            var obj = PathSelector.Executor.CachedRuntimeObject;

            for (var i = 0; i < eventItemList.EventItems.Length; i++)
            {
                var e = eventItemList.EventItems[i];
                if (!e.use) continue;

                var o = IsExtraEventItem(e) ? this : obj;

                var evInfo = e.GetEventInfo(o.GetType());

                evInfo.RemoveEventHandler(o, _cachedRuntimeDelegates[i]);
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
                t.Transfer(PathSelector.Executor.CachedRuntimeObject);
            }
        }


        #region ExtraEvents

        public event Action ThisEnabled;
        public event Action ThisDisabled;

        private IEnumerable<EventInfo> GetExtraEvents()
        {
            var type = GetType();
            return new[] { type.GetEvent(nameof(ThisEnabled)), type.GetEvent(nameof(ThisDisabled)) };
        }

        private static bool IsExtraEventItem(EventItem ei)
        {
            return EventItem.ExtractEventName(ei.EventName).StartsWith("This");
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
            position.height = (position.height - 4) / 3;
            position.width = fullWidth / 3 + 27;
            EditorGUI.PropertyField(position, sourceObjectMethodName, GUIContent.none);


            position.y += position.height + 2;
            position.width = fullWidth; // 3;
            // position.height = height * 2; // 3;
            EditorGUI.PropertyField(position, targetObject, GUIContent.none);

            // var btnRect = position;
            // btnRect.x += position.width + 2;
            // btnRect.width = 25;
            // TypeConstraintPropertyDrawer.Menu(btnRect, targetObject,
            //     go => go.GetComponents<Component>().Select(c => c as Object).Concat(new[] {go}).ToArray(), false);

            // btnRect.x = 27;
            position.y += position.height + 2;


            //Formatter
            const float w = 70;
            position.x += fullWidth - w * 1.65f;
            position.width = w * 1.65f;
            EditorGUI.LabelField(position, new GUIContent("Format"));
            position.x -= fullWidth - w * 1.65f;

            position.x += fullWidth - w + 1;
            position.width = w;
            EditorGUI.PropertyField(position, formatType, GUIContent.none);
            position.x -= fullWidth - w + 1;

            position.width = fullWidth / 3 * 2 - 27;
            EditorGUI.PropertyField(position, targetMethodName, GUIContent.none);


            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) * 3 + 4;
        }
    }
#endif
}