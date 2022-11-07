using UnityEngine;

namespace Gameplay
{
    public class CameraManager : MonoSelfBindingInjectable<CameraManager>
    {
        public Camera Camera => GetComponent<Camera>();
    }
}