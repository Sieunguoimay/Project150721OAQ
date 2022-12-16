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

        [SerializeField] private EventHandlerItem[] items;

        private void OnValidate()
        {
            var events = EventProviderType?.GetEvents();
            if (events == null)
            {
                items = new EventHandlerItem[0];
                return;
            }

            items ??= new EventHandlerItem[events.Length];

            if (items.Length != events.Length)
            {
                Array.Resize(ref items, events.Length);
            }

            for (var i = 0; i < events.Length; i++)
            {
                if (items[i] == null)
                {
                    items[i] = new EventHandlerItem();
                    items[i].UpdateEventName(events[i]);
                }
                else
                {
                    items[i].UpdateEventName(events[i]);
                }
            }
        }


        [ContextMenu("LogEvents")]
        public void LogEvents()
        {
            if (EventProviderType != null)
            {
                var events = EventProviderType.GetEvents();
                foreach (var e in events)
                {
                    Debug.Log(e.EventHandlerType.GenericTypeArguments.First().Name);
                }
            }
            else
            {
                Debug.Log("Not found");
            }
        }

        private Type EventProviderType => string.IsNullOrEmpty(path) ? sourceObject.GetType() : ReflectionUtility.GetTypeAtPath(sourceObject.GetType(), path.Split('.'));
        private object EventProviderObject => string.IsNullOrEmpty(path) ? sourceObject : ReflectionUtility.GetObjectAtPath(sourceObject, path.Split('.'));

        [System.Serializable]
        public class EventHandlerItem
        {
            [SerializeField, HideInInspector] private string eventName;

            [field: SerializeField] public MethodItem[] methodItems = new MethodItem[0];

            public string RuntimeEventName => ExtractEventName(eventName);

            public void UpdateEventName(EventInfo eventInfo)
            {
                eventName = FormatEventName(eventInfo);
            }

            public static string FormatEventName(EventInfo eventInfo)
            {
                return $"{eventInfo.Name}({string.Join(",", eventInfo.EventHandlerType.GenericTypeArguments.Select(arg => arg.Name))})";
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
                    evInfo.AddEventHandler(obj, CreateMethod(item.MethodInfo));//Delegate.CreateDelegate(evInfo.EventHandlerType, ));
                }
            }

            static Delegate CreateMethod(MethodInfo method)
            {
                if (method == null)
                {
                    throw new ArgumentNullException("method");
                }

                if (!method.IsStatic)
                {
                    throw new ArgumentException("The provided method must be static.", "method");
                }

                if (method.IsGenericMethod)
                {
                    throw new ArgumentException("The provided method must not be generic.", "method");
                }

                var parameters = method.GetParameters()
                    .Select(p => Expression.Parameter(p.ParameterType, p.Name))
                    .ToArray();
                var call = Expression.Call(null, method, parameters);
                return Expression.Lambda(call, parameters).Compile();
            }
            public void Unsubscribe(object obj)
            {
                if (methodItems.Length == 0) return;
                var evInfo = obj.GetType().GetEvent(RuntimeEventName);
                foreach (var item in methodItems)
                {
                    evInfo.RemoveEventHandler(obj, Delegate.CreateDelegate(evInfo.EventHandlerType, item.MethodInfo));
                }
            }
        }

        [Serializable]
        public class MethodItem
        {
            [SerializeField, UnityObjectSelector] private Object targetObject;

            [SerializeField, StringSelector(nameof(MethodNames))]
            public string methodName;

            public IEnumerable<string> MethodNames => targetObject.GetType().GetMethods(ReflectionUtility.MethodFlags).Where(m => m.ReturnType == typeof(void)).Select(DataBridge.FormatSetMethodName);
            public MethodInfo MethodInfo => targetObject ? DataBridge.GetMethodInfo(targetObject.GetType(), methodName) : null;
        }

        public bool ValidateEventHandlerItems()
        {
            var providerType = EventProviderType;

            foreach (var item in items)
            {
                if (item == null) continue;
                foreach (var mItem in item.methodItems)
                {
                    if (mItem.MethodInfo == null) return false;

                    var argTypes = providerType.GetEvent(item.RuntimeEventName).EventHandlerType.GetGenericArguments();
                    var methodParameters = mItem.MethodInfo.GetParameters();
                    if (argTypes.Length != methodParameters.Length) return false;
                    for (var i = 0; i < argTypes.Length; i++)
                    {
                        if (!methodParameters[i].ParameterType.IsAssignableFrom(argTypes[i])) return false;
                    }
                }
            }

            return true;
        }

        private void OnEnable()
        {
            foreach (var item in items)
            {
                item.Subscribe(EventProviderObject);
            }
        }

        private void OnDisable()
        {
            foreach (var item in items)
            {
                item.Unsubscribe(EventProviderObject);
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(EventSubscription))]
    public class EventSubscriptionEditor : Editor
    {
        private bool _error = false;

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

            for (int i = 0; i < items.arraySize; i++)
            {
                EditorGUILayout.PropertyField(items.GetArrayElementAtIndex(i).FindPropertyRelative("methodItems"), new GUIContent(items.GetArrayElementAtIndex(i).FindPropertyRelative("eventName").stringValue));
            }

            serializedObject.ApplyModifiedProperties();

            // base.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
            {
                _error = !((EventSubscription) target).ValidateEventHandlerItems();
            }

            if (GUILayout.Button("Validate"))
            {
                _error = !((EventSubscription) target).ValidateEventHandlerItems();
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