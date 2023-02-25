using System;
using UnityEngine;

namespace Common.UnityExtend.Reflection
{
    public class MethodInvoker:MonoBehaviour
    {
        [SerializeField] private UnityObjectPathSelector pathSelector;

        private void OnEnable()
        {
            pathSelector.Setup(true);
        }

        private void OnDisable()
        {
        }

        public void Invoke()
        {
            pathSelector.Executor.ExecutePath();
        }
    }
}