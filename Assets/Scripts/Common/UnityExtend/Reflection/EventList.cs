using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Common.UnityExtend.Attribute;
using UnityEditor;
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
    public class EventList<TEventItem> where TEventItem : EventItem, new()
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

    [Serializable]
    public class EventHandlerItem
    {
        [SerializeField, ComponentSelector] private Object targetObject;

        [SerializeField, StringSelector(nameof(GetMethodNames))]
        public string methodName;

        public IEnumerable<string> GetMethodNames() => targetObject == null
            ? null
            : ReflectionUtility.GetAllMethods(targetObject.GetType()).Where(m => m.ReturnType == typeof(void)).Select(ReflectionUtility.FormatName.FormatMethodName);

        public MethodInfo MethodInfo =>
            targetObject ? ReflectionUtility.GetMethodInfo(targetObject.GetType(), methodName, true) : null;

        public Delegate RuntimeHandler { get; private set; }

        public Delegate CreateDelegate(Type handlerType)
        {
            RuntimeHandler = CreateDelegate(handlerType, MethodInfo, targetObject);
            return RuntimeHandler;
        }

        public static Delegate CreateDelegate(Type handlerType, MethodInfo methodInfo, object targetObject)
        {
            Delegate runtimeHandler = null;
            if (methodInfo == null) return null;
            var prams = methodInfo.GetParameters();
            if (prams.Length == 0)
            {
                var methodCallExpression = Expression.Call(Expression.Constant(targetObject), methodInfo, null);
                if (handlerType == typeof(EventHandler))
                {
                    runtimeHandler = new EventHandler(Expression
                        .Lambda<Action<object, EventArgs>>(methodCallExpression,
                            Expression.Parameter(typeof(object)), Expression.Parameter(typeof(EventArgs)))
                        .Compile());
                }
                else
                {
                    var lambdaParamExpressions = handlerType.GetGenericArguments().Select(Expression.Parameter);
                    runtimeHandler = Expression.Lambda(handlerType, methodCallExpression, lambdaParamExpressions)
                        .Compile();
                }
            }
            else
            {
                runtimeHandler = Delegate.CreateDelegate(handlerType, targetObject, methodInfo);
            }

            return runtimeHandler;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(EventList<>))]
    public class EventListDrawer : PropertyDrawer
    {
        private bool _foldout;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.Box(position, GUIContent.none, GUI.skin.box);
            EditorGUI.BeginProperty(position, label, property);
            var eventItems = property.FindPropertyRelative("eventItems");
            var lines = _foldout ? eventItems.arraySize + 1 : 1;
            if (_foldout)
            {
                position.height -= 2;
            }

            position.height /= lines;

            _foldout = EditorGUI.Foldout(position, _foldout, "Events", true);
            if (_foldout)
            {
                position.y += position.height;
                position.x += 10;
                position.width -= 10;
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
            return base.GetPropertyHeight(property, label) * lines + (_foldout ? 2 : 0);
        }
    }
#endif
}