using System;
using Common.ResolveSystem;
using UnityEngine;

namespace Common
{
    public class CameraManager : MonoBehaviour
    {
        private void Awake()
        {
            Resolver.Instance.Bind(this);
        }

        private void OnDestroy()
        {
            Resolver.Instance.Unbind(this);
        }
    }
}