using UnityEngine;

namespace Gameplay
{
    public class CameraManager : InjectableBehaviour<CameraManager>
    {
        public Camera Camera => GetComponent<Camera>();
    }
}