using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Common.UnityExtend.Attribute;
using Common.UnityExtend.Reflection;
using Gameplay.Entities.Stage;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

namespace Common.UnityExtend
{
    public class EventSubscription : MonoBehaviour
    {
        [SerializeField, UnityObjectSelector] private Object sourceObject;

        [SerializeField, PathSelector(nameof(sourceObject))]
        private string path;

        [SerializeField] private bool extraEvents;

        [SerializeField] private EventItem[] items;

        [SerializeField] private bool useSameTypeSourceObjects;

        [SerializeField, UnityObjectSelector] private Object[] sameTypeSourceObjects;
        private IEnumerable<Object> SameTypeSourceObjects => useSameTypeSourceObjects ? sameTypeSourceObjects.Concat(new[] {sourceObject}) : new[] {sourceObject};

        private Type GetEventProviderType() => sourceObject == null
            ? null
            : string.IsNullOrEmpty(path)
                ? sourceObject.GetType()
                : ReflectionUtility.GetTypeAtPath(sourceObject.GetType(), path.Split('.'), true);

        private object GetEventProviderObject(object rootObject) =>
            string.IsNullOrEmpty(path)
                ? rootObject
                : ReflectionUtility.ExecutePathOfObject(rootObject, path.Split('.'), true);

        [System.Serializable]
        public class EventItem
        {
            [SerializeField, HideInInspector] private string eventName;

            [field: SerializeField] public EventHandlerItem[] methodItems = new EventHandlerItem[0];

            private readonly List<Delegate> _cachedRuntimeDelegates = new();
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

            public void Subscribe(object obj)
            {
                if (methodItems.Length == 0) return;
                var evInfo = GetEventInfo(obj.GetType());
                foreach (var item in methodItems)
                {
                    var runtimeDelegate = item.CreateDelegate(evInfo.EventHandlerType);
                    if (runtimeDelegate == null) continue;
                    evInfo.AddEventHandler(obj, runtimeDelegate);
                    _cachedRuntimeDelegates.Add(runtimeDelegate);
                }
            }

            public void Unsubscribe(object obj)
            {
                if (methodItems.Length == 0) return;
                var evInfo = GetEventInfo(obj.GetType());

                foreach (var d in _cachedRuntimeDelegates)
                {
                    evInfo.RemoveEventHandler(obj, d);
                }

                _cachedRuntimeDelegates.Clear();
            }

            public EventInfo GetEventInfo(Type type)
            {
                return type.GetEvents().FirstOrDefault(ei => FormatEventName(ei).Equals(eventName));
            }
        }

        [Serializable]
        public class EventHandlerItem
        {
            [SerializeField, UnityObjectSelector] private Object targetObject;

            [SerializeField, StringSelector(nameof(GetMethodNames))]
            public string methodName;

            public IEnumerable<string> GetMethodNames() =>
                targetObject.GetType().GetMethods(ReflectionUtility.MethodFlags)
                    .Where(m => m.ReturnType == typeof(void)).Select(ReflectionUtility.FormatName.FormatMethodName);

            public MethodInfo MethodInfo =>
                targetObject ? ReflectionUtility.GetMethodInfo(targetObject.GetType(), methodName, true) : null;

            public Delegate RuntimeHandler { get; private set; }

            public Delegate CreateDelegate(Type handlerType)
            {
                if (MethodInfo == null) return null;
                var prams = MethodInfo.GetParameters();
                if (prams.Length == 0)
                {
                    var methodCallExpression = Expression.Call(Expression.Constant(targetObject), MethodInfo, null);
                    if (handlerType == typeof(EventHandler))
                    {
                        RuntimeHandler = new EventHandler(Expression
                            .Lambda<Action<object, EventArgs>>(methodCallExpression,
                                Expression.Parameter(typeof(object)), Expression.Parameter(typeof(EventArgs)))
                            .Compile());
                    }
                    else
                    {
                        var lambdaParamExpressions = handlerType.GetGenericArguments().Select(Expression.Parameter);
                        RuntimeHandler = Expression.Lambda(handlerType, methodCallExpression, lambdaParamExpressions)
                            .Compile();
                    }
                }
                else
                {
                    RuntimeHandler = Delegate.CreateDelegate(handlerType, targetObject, MethodInfo);
                }

                return RuntimeHandler;
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

        private static bool IsExtraEventItem(EventItem ei)
        {
            return EventItem.ExtractEventName(ei.EventName).StartsWith("This");
        }

        #endregion

#if UNITY_EDITOR
        [ContextMenu("UpdateEventItems")]
        public void UpdateEventItems()
        {
            var events = GetEventProviderType()?.GetEvents().Concat(extraEvents ? GetExtraEvents() : new EventInfo[0]).ToArray();

            if (events == null || events.Length == 0)
            {
                items = new EventItem[0];
                return;
            }

            if (items != null && items.Length == events.Length) return;

            var newItems = new EventItem[events.Length];

            for (var i = 0; i < events.Length; i++)
            {
                var n = EventItem.FormatEventName(events[i]);
                var item = items?.FirstOrDefault(it => it.EventName.Equals(n));
                if (item != null)
                {
                    newItems[i] = item;
                }
                else
                {
                    newItems[i] = new EventItem();
                    newItems[i].UpdateEventIdentity(events[i]);
                }
            }

            items = newItems;
        }

        public bool ValidateEventHandlerItems()
        {
            var providerType = GetEventProviderType();
            if (providerType == null) return true;
            foreach (var item in items)
            {
                if (item == null) continue;
                var pType = IsExtraEventItem(item) ? GetType() : providerType;

                foreach (var mItem in item.methodItems)
                {
                    if (mItem.MethodInfo == null) return false;

                    var argTypes = item.GetEventInfo(pType).EventHandlerType.GetGenericArguments();
                    var methodParameters = mItem.MethodInfo.GetParameters();
                    if (methodParameters.Length <= 0) continue;
                    if (argTypes.Length != methodParameters.Length) return false;
                    if (argTypes.Where((t, i) => !methodParameters[i].ParameterType.IsAssignableFrom(t)).Any())
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void RefreshSubscription()
        {
            foreach (var rootObject in SameTypeSourceObjects)
            {
                var providerObject = GetEventProviderObject(rootObject);
                foreach (var item in items)
                {
                    var obj = IsExtraEventItem(item) ? this : providerObject;
                    item.Unsubscribe(obj);
                    item.Subscribe(obj);
                }
            }
        }

        public bool ValidateSameTypeSourceObjects()
        {
            return !sourceObject || (SameTypeSourceObjects?.All(rootObject => rootObject.GetType() == sourceObject.GetType()) ?? true);
        }
#endif

        private void OnEnable()
        {
            foreach (var rootObject in SameTypeSourceObjects)
            {
                var providerObject = GetEventProviderObject(rootObject);
                foreach (var item in items)
                {
                    var obj = IsExtraEventItem(item) ? this : providerObject;
                    item.Subscribe(obj);
                }
            }

            ThisEnabled?.Invoke();
        }

        private void OnDisable()
        {
            foreach (var rootObject in SameTypeSourceObjects)
            {
                var providerObject = GetEventProviderObject(rootObject);
                foreach (var item in items)
                {
                    var obj = IsExtraEventItem(item) ? this : providerObject;
                    item.Unsubscribe(obj);
                }
            }

            ThisDisabled?.Invoke();
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(EventSubscription))]
    public class EventSubscriptionEditor : Editor
    {
        private bool _error = false;
        private bool _anythingChanged = true;
        private SerializedProperty _sourceObject;
        private SerializedProperty _path;
        private SerializedProperty _extraEvents;
        private SerializedProperty _sameTypeSourceObjects;
        private SerializedProperty _useSameTypeSourceObjects;
        private SerializedProperty _items;

        private void OnEnable()
        {
            _error = !((EventSubscription) target).ValidateEventHandlerItems();

            _sourceObject = serializedObject.FindProperty("sourceObject");
            _path = serializedObject.FindProperty("path");
            _extraEvents = serializedObject.FindProperty("extraEvents");
            _useSameTypeSourceObjects = serializedObject.FindProperty("useSameTypeSourceObjects");
            _sameTypeSourceObjects = serializedObject.FindProperty("sameTypeSourceObjects");
            _items = serializedObject.FindProperty("items");
        }

        public override void OnInspectorGUI()
        {
            var es = (EventSubscription) target;
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour(es), typeof(EventSubscription), false);
            GUI.enabled = true;


            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(_sourceObject);
            EditorGUILayout.PropertyField(_path);
            EditorGUILayout.PropertyField(_extraEvents);

            // if (EditorGUI.EndChangeCheck())
            // {
            //     es.UpdateEventItems();
            //     _anythingChanged = true;
            // }

            EditorGUILayout.PropertyField(_useSameTypeSourceObjects);

            if (_useSameTypeSourceObjects.boolValue)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(_sameTypeSourceObjects);
                if (EditorGUI.EndChangeCheck())
                {
                    _error = !es.ValidateSameTypeSourceObjects();
                }
            }

            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
            {
                _anythingChanged = true;
                return;
            }

            EditorGUI.BeginChangeCheck();


            for (var i = 0; i < _items.arraySize; i++)
            {
                EditorGUILayout.PropertyField(_items.GetArrayElementAtIndex(i).FindPropertyRelative("methodItems"), new GUIContent(_items.GetArrayElementAtIndex(i).FindPropertyRelative("eventName").stringValue));
            }

            serializedObject.ApplyModifiedProperties();

            // base.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
            {
                _anythingChanged = true;
                return;
            }

            try
            {
                if (GUILayout.Button("Validate"))
                {
                    _anythingChanged = true;
                }
            }
            catch (Exception)
            {
                //
            }

            if (_anythingChanged)
            {
                _anythingChanged = false;
                es.UpdateEventItems();
                _error = !es.ValidateEventHandlerItems();
                _error |= !es.ValidateSameTypeSourceObjects();
                if (Application.isPlaying)
                {
                    es.RefreshSubscription();
                }
            }

            if (_error)
            {
                var prevColor = GUI.color;
                GUI.color = Color.red;
                EditorGUILayout.LabelField("Err");
                GUI.color = prevColor;
            }
        }
    }
#endif
}