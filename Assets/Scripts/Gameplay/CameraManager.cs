using UnityEngine;

namespace Gameplay
{
    public class CameraManager : MonoBindingInjectable<CameraManager>
    {
        public Camera Camera => GetComponent<Camera>();
    }
}