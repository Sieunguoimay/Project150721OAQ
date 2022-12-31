using System;
using System.Reflection;
using Common.Curve.PathCreator.Core.Editor.Helper;
using Common.UnityExtend.Attribute;
using UnityEditor;
using UnityEngine;

namespace Common.UnityExtend.Reflection
{
    [Serializable]
    public class UnityObjectPathSelector
    {
        [SerializeField, ComponentSelector] private UnityEngine.Object sourceObject;

#if UNITY_EDITOR
        [PathSelector(nameof(sourceObject))]
#endif
        [SerializeField]
        private string path;

        [field: System.NonSerialized] public PathExecutor Executor { get; private set; }

        public Type PathFinalType => ReflectionUtility.GetTypeAtPath(sourceObject?.GetType(),
            string.IsNullOrEmpty(path) ? new string[0] : path.Split('.'), true);

        public void Setup(bool cache)
        {
            Executor = new PathExecutor();
            Executor.Setup(path, sourceObject, cache);
        }

        public class PathExecutor
        {
            private MemberInfoWrapper[] _memberInfos;
            private object _sourceObject;

            private bool _cache;
            private object _runtimeObject;

            public object CachedRuntimeObject //=> ExecutePath();
            {
                get
                {
                    if (_cache)
                    {
                        return _runtimeObject ??= ExecutePath();
                    }

                    return ExecutePath();
                }
            }

            public void Setup(string path, object sourceObject, bool cache)
            {
                _sourceObject = sourceObject;
                if (string.IsNullOrEmpty(path))
                {
                    _memberInfos = new MemberInfoWrapper[0];
                    return;
                }

                var p = path.Split('.');
                _memberInfos = new MemberInfoWrapper[p.Length];
                var currType = _sourceObject.GetType();
                for (var i = 0; i < p.Length; i++)
                {
                    _memberInfos[i] = new MemberInfoWrapper();
                    _memberInfos[i].Setup(currType, p[i], true);
                    currType = _memberInfos[i].GetMemberType();
                }

                _cache = cache;
            }

            public object ExecutePath()
            {
                var currObj = _sourceObject;

                foreach (var mi in _memberInfos)
                {
                    currObj = mi.GetMemberValue(currObj);
                }

                return currObj;
            }

            public Type RuntimePathFinalType =>
                _memberInfos.Length == 0 ? _sourceObject.GetType() : _memberInfos[^1].GetMemberType();
        }

        public class MemberInfoWrapper
        {
            private FieldInfo _fieldInfo;
            private PropertyInfo _propertyInfo;
            private MethodInfo _methodInfo;

            public void Setup(Type type, string formattedName, bool isNameFormatted)
            {
                if (type == null) return;
                _fieldInfo = ReflectionUtility.GetFieldInfo(type, formattedName, isNameFormatted);
                if (_fieldInfo != null) return;
                _propertyInfo = ReflectionUtility.GetPropertyInfo(type, formattedName, isNameFormatted);
                if (_propertyInfo != null) return;
                _methodInfo = ReflectionUtility.GetMethodInfo(type, formattedName, isNameFormatted);
            }

            public Type GetMemberType()
            {
                if (_fieldInfo != null) return _fieldInfo.FieldType;
                if (_propertyInfo != null) return _propertyInfo.PropertyType;
                if (_methodInfo != null) return _methodInfo.ReturnType;
                return null;
            }

            public object GetMemberValue(object obj)
            {
                if (_fieldInfo != null) return _fieldInfo.GetValue(obj);
                if (_propertyInfo != null) return _propertyInfo.GetValue(obj);
                if (_methodInfo != null) return _methodInfo.Invoke(obj, null);
                return null;
            }
        }

        public class CompactAttribute : PropertyAttribute
        {
            
        }
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(UnityObjectPathSelector))]
    public class UnityObjectPathSelectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            position.height -= 2;
            position.height /= 2;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("sourceObject"));
            position.y += position.height + 2;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("path"));
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) * 2 + 2;
        }
    }

    [CustomPropertyDrawer(typeof(UnityObjectPathSelector.CompactAttribute))]
    public class UnityObjectPathSelectorCompactDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            position.width /= 2;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("sourceObject"), GUIContent.none);
            position.x += position.width;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("path"), GUIContent.none);
            EditorGUI.EndProperty();
        }
    }
#endif
}