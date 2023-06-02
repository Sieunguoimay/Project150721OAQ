#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Common.UnityExtend.Attribute;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Common.UnityExtend.Reflection
{
    public class EventSubscription : MonoBehaviour
    {
        [SerializeField, ComponentSelector] private Object sourceObject;

        [SerializeField, PathSelector(nameof(sourceObject))]
        private string path;

        [SerializeField] private bool extraEvents;

        [SerializeField] private EventList<EventItemWithTargets> itemList;

        [SerializeField] private bool useSameTypeSourceObjects;

        [SerializeField, ComponentSelector] private Object[] sameTypeSourceObjects;
        private IEnumerable<Object> SameTypeSourceObjects => useSameTypeSourceObjects ? sameTypeSourceObjects.Concat(new[] { sourceObject }) : new[] { sourceObject };

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
        public class EventItemWithTargets : EventItem
        {
            [field: SerializeField] public EventHandlerItem[] methodItems = new EventHandlerItem[0];

            private readonly List<Delegate> _cachedRuntimeDelegates = new();

            public void Subscribe(object obj)
            {
                if (methodItems.Length == 0 || obj == null) return;
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
                if (methodItems.Length == 0 || obj == null) return;
                var evInfo = GetEventInfo(obj.GetType());

                foreach (var d in _cachedRuntimeDelegates)
                {
                    evInfo.RemoveEventHandler(obj, d);
                }

                _cachedRuntimeDelegates.Clear();
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

#if UNITY_EDITOR
        [ContextMenu("UpdateEventItems")]
        public void UpdateEventItems()
        {
            itemList.ValidateEventItems(GetEventProviderType()?.GetEvents().Concat(extraEvents ? GetExtraEvents() : new EventInfo[0]).ToArray());
        }

        public bool ValidateEventHandlerItems()
        {
            var providerType = GetEventProviderType();
            if (providerType == null || itemList == null) return true;
            foreach (var item in itemList.EventItems)
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
                foreach (var item in itemList.EventItems)
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
                foreach (var item in itemList.EventItems)
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
                foreach (var item in itemList.EventItems)
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

        // private SerializedProperty _items;
        private SerializedProperty _itemList;

        private void OnEnable()
        {
            _error = !((EventSubscription)target).ValidateEventHandlerItems();

            _sourceObject = serializedObject.FindProperty("sourceObject");
            _path = serializedObject.FindProperty("path");
            _extraEvents = serializedObject.FindProperty("extraEvents");
            _useSameTypeSourceObjects = serializedObject.FindProperty("useSameTypeSourceObjects");
            _sameTypeSourceObjects = serializedObject.FindProperty("sameTypeSourceObjects");
            // _items = serializedObject.FindProperty("items"); //.FindPropertyRelative("eventItems");
            _itemList = serializedObject.FindProperty("itemList").FindPropertyRelative("eventItems");
        }

        public override void OnInspectorGUI()
        {
            if (_sourceObject == null) return;
            var es = (EventSubscription)target;
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour(es), typeof(EventSubscription), false);
            GUI.enabled = true;


            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(_sourceObject);
            EditorGUILayout.PropertyField(_path);
            EditorGUILayout.PropertyField(_extraEvents);

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
            try
            {
                for (var i = 0; i < _itemList.arraySize; i++)
                {
                    EditorGUILayout.PropertyField(_itemList.GetArrayElementAtIndex(i).FindPropertyRelative("methodItems"), new GUIContent(_itemList.GetArrayElementAtIndex(i).FindPropertyRelative("eventName").stringValue));
                }
            }
            catch (Exception)
            {
                //
            }

            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck())
            {
                _anythingChanged = true;
                return;
            }

            try
            {
                var alignment = GUI.skin.button.alignment;
                GUI.skin.button.alignment = TextAnchor.MiddleCenter;

                if (GUILayout.Button("Validate"))
                {
                    _anythingChanged = true;
                }

                GUI.skin.button.alignment = alignment;
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