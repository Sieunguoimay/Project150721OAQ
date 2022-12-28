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
    [Serializable]
    public class EventItem
    {
        [SerializeField] protected string eventName;

        public string EventName => eventName;

        public void UpdateEventIdentity(EventInfo eventInfo)
        {
            eventName = FormatEventName(eventInfo);
        }

        public static string FormatEventName(EventInfo eventInfo)
        {
            return
                $"{eventInfo.Name}({string.Join(",", eventInfo.EventHandlerType.GenericTypeArguments.Select(arg => arg.Name))})";
        }

        public static string ExtractEventName(string formattedName)
        {
            return formattedName.Split('(').FirstOrDefault();
        }

        public EventInfo GetEventInfo(Type type)
        {
            return type.GetEvents().FirstOrDefault(ei => FormatEventName(ei).Equals(eventName));
        }
    }

    [Serializable]
    public class EventItemList<TEventItem> where TEventItem : EventItem, new()
    {
        [SerializeField] private TEventItem[] eventItems;

        public TEventItem[] EventItems => eventItems;


        public void ValidateEventItems(IReadOnlyList<EventInfo> events)
        {
            if (events == null || events.Count == 0)
            {
                eventItems = new TEventItem[0];
                return;
            }

            if (eventItems != null && eventItems.Length == events.Count) return;
            var newItems = new TEventItem[events.Count];

            for (var i = 0; i < events.Count; i++)
            {
                var n = EventItem.FormatEventName(events[i]);
                var item = eventItems?.FirstOrDefault(it => it.EventName.Equals(n));
                if (item != null)
                {
                    newItems[i] = item;
                }
                else
                {
                    newItems[i] = new TEventItem();
                    newItems[i].UpdateEventIdentity(events[i]);
                }
            }

            eventItems = newItems;
        }
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(EventItemList<>))]
    public class EventItemListDrawer : PropertyDrawer
    {
        private bool _foldout;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var eventItems = property.FindPropertyRelative("eventItems");
            var lines = _foldout ? eventItems.arraySize + 1 : 1;

            position.height /= lines;
            position.height -= lines * 2;

              _foldout = EditorGUI.Foldout(position, _foldout, "Events");
            if (_foldout)
            {
                position.y += position.height;
                position.x += 10;
                for (var i = 0; i < eventItems.arraySize; i++)
                {
                    EditorGUI.PropertyField(position, eventItems.GetArrayElementAtIndex(i), GUIContent.none);
                    position.y += position.height;
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var lines = _foldout ? property.FindPropertyRelative("eventItems").arraySize + 1 : 1;
            return base.GetPropertyHeight(property, label) * lines + lines * 2;
        }
    }
#endif

    public class DataBridge : MonoBehaviour
    {
        [SerializeField, ComponentSelector] private Object sourceObject;

        [SerializeField, PathSelector(nameof(sourceObject))]
        private string path;

        [SerializeField] private EventItemList<SourceEventItem> eventItemList;

        [SerializeField] private MethodPairItem[] items;

        public Object SourceObject => ReflectionUtility.ExecutePathOfObject(sourceObject, string.IsNullOrEmpty(path) ? new string[0] : path.Split('.'), true) as Object;
        public Type SourceObjectType => ReflectionUtility.GetTypeAtPath(sourceObject?.GetType(), string.IsNullOrEmpty(path) ? new string[0] : path.Split('.'), true);

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
                _sourceMethodInfo = ReflectionUtility.GetTypeAtPath(sourceObject.GetType(), sourceObjectMethodName.Split('.'), true);

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

            var events = SourceObjectType?.GetEvents().Concat(GetExtraEvents()).ToArray();
            eventItemList.ValidateEventItems(events);
        }

        private void OnEnable()
        {
            foreach (var t in items)
            {
                t.SetupReflection(SourceObject);
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
            var obj = SourceObject;
            _cachedRuntimeDelegates = new Delegate[eventItemList.EventItems.Length];
            for (var i = 0; i < eventItemList.EventItems.Length; i++)
            {
                var e = eventItemList.EventItems[i];
                if (!e.use) continue;

                var evInfo = e.GetEventInfo(obj.GetType());

                var runtimeDelegate = EventSubscription.EventHandlerItem.CreateDelegate(evInfo.EventHandlerType, MethodInfo, this);
                if (runtimeDelegate == null) continue;

                evInfo.AddEventHandler(obj, runtimeDelegate);
                _cachedRuntimeDelegates[i] = runtimeDelegate;
            }
        }

        private void Unsubscribe()
        {
            var obj = SourceObject;

            for (var i = 0; i < eventItemList.EventItems.Length; i++)
            {
                var e = eventItemList.EventItems[i];
                if (!e.use) continue;

                var evInfo = e.GetEventInfo(obj.GetType());

                evInfo.RemoveEventHandler(obj, _cachedRuntimeDelegates[i]);
            }

            _cachedRuntimeDelegates = null;
        }

        private MethodInfo MethodInfo => GetType().GetMethod("OnEventTriggered");

        private void OnEventTriggered()
        {
            Debug.Log("OK");
        }

        [ContextMenu("Transfer")]
        public void Transfer()
        {
            foreach (var t in items)
            {
                t.Transfer(SourceObject);
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