using UnityEngine;

namespace Gameplay
{
    public class CameraManager : MonoInjectable<CameraManager>
    {
        public Camera Camera => GetComponent<Camera>();
    }
}