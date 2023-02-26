using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common.UnityExtend.Attribute;
using UnityEngine;

namespace Common.UnityExtend.Reflection
{
    public class MethodInvoker : MonoBehaviour
    {
        [SerializeField, ComponentSelector] private UnityEngine.Object sourceObject;

        [SerializeField]
#if UNITY_EDITOR
        [StringSelector(nameof(MethodNames))]
#endif
        private string methodName;

#if UNITY_EDITOR
        public IEnumerable<string> MethodNames => sourceObject.GetType().GetMethods(ReflectionUtility.MethodFlags)
            .Select(ReflectionUtility.FormatName.FormatMethodName);

#endif
        private MethodInfo _methodInfo;
        private void OnEnable()
        {
            _methodInfo = ReflectionUtility.GetMethodInfo(sourceObject.GetType(), methodName, true);
        }

        private void OnDisable()
        {
            _methodInfo = null;
        }

        public void Invoke()
        {
            _methodInfo?.Invoke(sourceObject, Array.Empty<object>());
        }
    }
}