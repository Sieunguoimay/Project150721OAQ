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
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace Common.UnityExtend.Reflection
{
    public interface IDataBridgeTrigger
    {
        UnityEvent<int> EventTrigger { get; }
        IReadOnlyList<string> Filters { get; }
    }

/*
    [field: System.NonSerialized] public UnityEvent<int> EventTrigger { get; private set; } = new();
    public IReadOnlyList<string> Filters => new[] {"static", "state", "progress"};
*/

    public class DataBridge : MonoBehaviour
    {
        [SerializeField] private bool useFreeObject = false;

        [SerializeField, TypeConstraint(false, typeof(IDataBridgeTrigger))]
        private Object sourceObject;

        [SerializeField, UnityObjectSelector] private Object freeSourceObject;

        [SerializeField, StringSelector(nameof(EventFilters))]
        private string eventFilter;

        [SerializeField] private Item[] items;

#if UNITY_EDITOR
        public IEnumerable<string> SourceMethodNames
        {
            get
            {
                return SourceObject.GetType()
                    .GetMethods().Where(m => m.GetParameters().Length == 0 && m.ReturnType != typeof(void))
                    .Select(FormatGetMethodName);
            }
        }
#endif
        [Serializable]
        public class Item
        {
#if UNITY_EDITOR

            [StringSelector(nameof(SourceMethodNames), true)]
#endif
            [SerializeField]
            private string sourceObjectMethodName;


            [SerializeField] private Object targetObject;


#if UNITY_EDITOR
            public IEnumerable<string> TargetMethodNames
            {
                get
                {
                    return targetObject.GetType()?
                        .GetMethods().Where(m => m.GetParameters().Length <= 1 && m.ReturnType == typeof(void))
                        .Select(FormatSetMethodName);
                }
            }

            [StringSelector(nameof(TargetMethodNames))]
#endif
            [SerializeField]
            private string targetMethodName;

            private MethodInfo _targetMethodInfo;
            private MethodInfo _sourceMethodInfo;

            public void Transfer(Object sourceObject)
            {
                SetupReflection(sourceObject);

                if (_targetMethodInfo.GetParameters().Length > 0)
                {
                    _targetMethodInfo.Invoke(targetObject, new[] {_sourceMethodInfo.Invoke(sourceObject, new object[0])});
                }
                else
                {
                    _targetMethodInfo.Invoke(targetObject, new object[0]);
                }
            }


            public void SetupReflection(Object sourceObject)
            {
                if (_targetMethodInfo != null && _sourceMethodInfo != null) return;

                _targetMethodInfo = targetObject.GetType().GetMethods()
                    .FirstOrDefault(m => FormatSetMethodName(m).Equals(targetMethodName));

                _sourceMethodInfo = sourceObject.GetType().GetMethods()
                    .FirstOrDefault(m => FormatGetMethodName(m).Equals(sourceObjectMethodName));

                if (_targetMethodInfo == null || _sourceMethodInfo == null)
                {
                    Debug.LogError("Methods not found");
                    return;
                }

                var t = _targetMethodInfo.GetParameters().FirstOrDefault()?.ParameterType;
                if (!t?.IsAssignableFrom(_sourceMethodInfo.ReturnType) ?? false)
                {
                    Debug.LogError(
                        $"Type mismatch  {_sourceMethodInfo.ReturnType} is not assignable to {_targetMethodInfo.GetParameters()[0].ParameterType}");
                }
            }

            public void Clear()
            {
                _targetMethodInfo = null;
                _sourceMethodInfo = null;
            }
        }

        private IDataBridgeTrigger _source;
        private IDataBridgeTrigger Source => _source ??= SourceObject as IDataBridgeTrigger;

        public IReadOnlyList<string> EventFilters => Source?.Filters;

        public Object SourceObject => freeSourceObject ? freeSourceObject : sourceObject;

        private int _eventFilterMask;

        private void OnValidate()
        {
            if (items != null)
            {
                for (var i = 0; i < items.Length; i++)
                {
                    items[i].Clear();
                }
            }
        }

        private void OnEnable()
        {
            for (var i = 0; i < items.Length; i++)
            {
                items[i].SetupReflection(SourceObject);
            }

            if (!useFreeObject)
            {
                Source.EventTrigger.AddListener(OnSourceTrigger);
                for (var i = 0; i < EventFilters.Count; i++)
                {
                    if (EventFilters[i].Equals(eventFilter))
                    {
                        _eventFilterMask = (int) Mathf.Pow(2f, i); //1 2 4 8 16
                        break;
                    }
                }
            }
        }

        private void OnDisable()
        {
            if (!useFreeObject)
            {
                Source.EventTrigger.RemoveListener(OnSourceTrigger);
            }
        }

        private void OnSourceTrigger(int mask)
        {
            if ((_eventFilterMask & mask) != 0)
            {
                Transfer();
            }
        }

        [ContextMenu("Transfer")]
        public void Transfer()
        {
            for (var i = 0; i < items.Length; i++)
            {
                items[i].Transfer(SourceObject);
            }
        }

        private static string FormatGetMethodName(MethodInfo p)
        {
            return $"{p.Name}(): {p.ReturnType.Name}";
        }

        public static string FormatSetMethodName(MethodBase p)
        {
            return $"{p.Name}({string.Join(',',p.GetParameters().Select(pi=>pi.ParameterType.Name))})";
        }

        public static MethodInfo GetMethodInfo(Type type, string formattedName)
        {
            return type.GetMethod(formattedName.Split('(').FirstOrDefault() ?? string.Empty, ReflectionUtility.MethodFlags);
        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(DataBridge))]
    [CanEditMultipleObjects]
    public class DataBridgeEditor : Editor
    {
        private SerializedProperty _eventFilter;
        private SerializedProperty _sourceObject;
        private SerializedProperty _useFreeObject;
        private SerializedProperty _freeSourceObject;
        private SerializedProperty _items;

        private void OnEnable()
        {
            _eventFilter = serializedObject.FindProperty("eventFilter");
            _sourceObject = serializedObject.FindProperty("sourceObject");
            _freeSourceObject = serializedObject.FindProperty("freeSourceObject");
            _useFreeObject = serializedObject.FindProperty("useFreeObject");
            _items = serializedObject.FindProperty("items");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_useFreeObject);
            if (_useFreeObject.boolValue)
            {
                EditorGUILayout.PropertyField(_freeSourceObject);
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                var width = EditorGUIUtility.currentViewWidth - 40;
                EditorGUILayout.PropertyField(_sourceObject, GUILayout.Width(width / 3 * 2));
                EditorGUILayout.PropertyField(_eventFilter, GUIContent.none, GUILayout.Width(width / 3 * 1));
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_items);
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomPropertyDrawer(typeof(DataBridge.Item))]
    public class DataBridgeItemDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var sourceObjectMethodName = property.FindPropertyRelative("sourceObjectMethodName");
            var targetObject = property.FindPropertyRelative("targetObject");
            var targetMethodName = property.FindPropertyRelative("targetMethodName");

            var fullWidth = position.width;
            var fullHeight = position.height;
            position.height = fullHeight / 2 - 2;
            position.width = fullWidth / 3 + 27;
            EditorGUI.PropertyField(position, sourceObjectMethodName, GUIContent.none);

            position.y += position.height;

            position.width = fullWidth / 3;
            EditorGUI.PropertyField(position, targetObject, GUIContent.none);

            var btnRect = position;
            btnRect.x += position.width + 2;
            btnRect.width = 25;
            TypeConstraintPropertyDrawer.Menu(btnRect, targetObject, go => go.GetComponents<Component>().Select(c => c as Object).Concat(new[] {go}).ToArray(), false);

            btnRect.x += 27;
            btnRect.width = fullWidth / 3 * 2 - 27;
            EditorGUI.PropertyField(btnRect, targetMethodName, GUIContent.none);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) * 2 + 4;
        }
    }
#endif
}