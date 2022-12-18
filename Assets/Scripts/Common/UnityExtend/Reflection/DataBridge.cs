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
    public class DataBridge : MonoBehaviour
    {
        [SerializeField, UnityObjectSelector] private Object sourceObject;

        [SerializeField] private MethodPairItem[] items;

// #if UNITY_EDITOR
//
//         public IEnumerable<string> GetSourceMethodNames()
//         {
//             return sourceObject.GetType()
//                 .GetMethods().Where(m => m.GetParameters().Length == 0 && m.ReturnType != typeof(void))
//                 .Select(FormatGetMethodName);
//         }
// #endif
        [Serializable]
        public class MethodPairItem
        {
#if UNITY_EDITOR

            [PathSelector(nameof(sourceObject), true, true)]
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

            private MethodInfo _targetMethodInfo;
            private Type _sourceMethodInfo;
            private int _numMethodParameters = 0;

            public void Transfer(Object sourceObject)
            {
                SetupReflection(sourceObject);

                _targetMethodInfo.Invoke(targetObject, _numMethodParameters == 1
                    ? new[]
                    {
                        ReflectionUtility.ExecutePathOfObject(sourceObject, sourceObjectMethodName.Split('.'), true)
                    }
                    : new object[0]);
            }

            public void SetupReflection(Object sourceObject)
            {
                if (_targetMethodInfo != null && _sourceMethodInfo != null) return;

                _targetMethodInfo = targetObject.GetType()
                    .GetMethod(ReflectionUtility.FormatName.ExtractMethodName(targetMethodName),
                        ReflectionUtility.MethodFlags);
                // _sourceMethodInfo = sourceObject.GetType().GetMethod(ReflectionUtility.FormatName.ExtractMethodName(sourceObjectMethodName),
                //     ReflectionUtility.MethodFlags);
                _sourceMethodInfo =
                    ReflectionUtility.GetTypeAtPath(sourceObject.GetType(), sourceObjectMethodName.Split('.'), true);

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
        }


        private int _eventFilterMask;

        private void OnValidate()
        {
            if (items == null) return;
            foreach (var t in items)
            {
                t.Clear();
            }
        }

        private void OnEnable()
        {
            foreach (var t in items)
            {
                t.SetupReflection(sourceObject);
            }
        }

        [ContextMenu("Transfer")]
        public void Transfer()
        {
            foreach (var t in items)
            {
                t.Transfer(sourceObject);
            }
        }

        public static MethodInfo GetMethodInfo(Type type, string formattedName)
        {
            return type.GetMethod(formattedName.Split('(').FirstOrDefault() ?? string.Empty,
                ReflectionUtility.MethodFlags);
        }
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(DataBridge.MethodPairItem))]
    public class DataBridgeItemDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
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