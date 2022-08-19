using System;
using Common.ResolveSystem;
using UnityEngine;

namespace Common
{
    public class CameraManager : MonoBehaviour
    {
        public Camera Camera => GetComponent<Camera>();
    }
}