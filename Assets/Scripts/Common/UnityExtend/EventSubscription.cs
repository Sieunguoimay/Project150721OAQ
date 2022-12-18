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

        [SerializeField] private EventItem[] items;

        private Type GetEventProviderType() => sourceObject == null
            ? null
            : string.IsNullOrEmpty(path)
                ? sourceObject.GetType()
                : ReflectionUtility.GetTypeAtPath(sourceObject.GetType(), path.Split('.'), true);

        private object GetEventProviderObject() =>
            string.IsNullOrEmpty(path)
                ? sourceObject
                : ReflectionUtility.ExecutePathOfObject(sourceObject, path.Split('.'), true);

        [System.Serializable]
        public class EventItem
        {
            [SerializeField, HideInInspector] private string eventName;

            [field: SerializeField] public EventHandlerItem[] methodItems = new EventHandlerItem[0];

            private readonly List<Delegate> _cachedRuntimeDelegates = new();
            public string RuntimeEventName => ExtractEventName(eventName);

            public void UpdateEventName(EventInfo eventInfo)
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
                var evInfo = obj.GetType().GetEvent(RuntimeEventName);
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
                var evInfo = obj.GetType().GetEvent(RuntimeEventName);

                foreach (var d in _cachedRuntimeDelegates)
                {
                    evInfo.RemoveEventHandler(obj, d);
                }

                _cachedRuntimeDelegates.Clear();
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
                targetObject ? DataBridge.GetMethodInfo(targetObject.GetType(), methodName) : null;

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
#if UNITY_EDITOR
        public void UpdateEventItems()
        {
            var events = GetEventProviderType()?.GetEvents();
            if (events == null || events.Length == 0)
            {
                items = new EventItem[0];
                return;
            }

            items ??= new EventItem[events.Length];

            if (items.Length != events.Length)
            {
                Array.Resize(ref items, events.Length);
            }

            for (var i = 0; i < events.Length; i++)
            {
                if (items[i] == null)
                {
                    items[i] = new EventItem();
                    items[i].UpdateEventName(events[i]);
                }
                else
                {
                    items[i].UpdateEventName(events[i]);
                }
            }
        }

        public bool ValidateEventHandlerItems()
        {
            var providerType = GetEventProviderType();
            if (providerType == null) return true;
            foreach (var item in items)
            {
                if (item == null) continue;
                foreach (var mItem in item.methodItems)
                {
                    if (mItem.MethodInfo == null) return false;

                    var argTypes = providerType.GetEvent(item.RuntimeEventName).EventHandlerType.GetGenericArguments();
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
            foreach (var item in items)
            {
                item.Unsubscribe(GetEventProviderObject());
                item.Subscribe(GetEventProviderObject());
            }
        }
#endif
        private void OnEnable()
        {
            foreach (var item in items)
            {
                item.Subscribe(GetEventProviderObject());
            }
        }

        private void OnDisable()
        {
            foreach (var item in items)
            {
                item.Unsubscribe(GetEventProviderObject());
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(EventSubscription))]
    public class EventSubscriptionEditor : Editor
    {
        private bool _error = false;
        private bool _anythingChanged = true;

        private void OnEnable()
        {
            _error = !((EventSubscription) target).ValidateEventHandlerItems();
        }

        public override void OnInspectorGUI()
        {
            var es = (EventSubscription) target;
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour(es), typeof(EventSubscription), false);
            GUI.enabled = true;

            var sourceObject = serializedObject.FindProperty("sourceObject");
            var path = serializedObject.FindProperty("path");
            var items = serializedObject.FindProperty("items");

            EditorGUI.BeginChangeCheck();
            serializedObject.Update();

            EditorGUILayout.PropertyField(sourceObject);
            EditorGUILayout.PropertyField(path);

            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
            {
                es.UpdateEventItems();
                _anythingChanged = true;
            }

            EditorGUI.BeginChangeCheck();

            for (int i = 0; i < items.arraySize; i++)
            {
                EditorGUILayout.PropertyField(items.GetArrayElementAtIndex(i).FindPropertyRelative("methodItems"),
                    new GUIContent(items.GetArrayElementAtIndex(i).FindPropertyRelative("eventName").stringValue));
            }

            serializedObject.ApplyModifiedProperties();

            // base.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
            {
                _anythingChanged = true;
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
                _error = !((EventSubscription) target).ValidateEventHandlerItems();
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