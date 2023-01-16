using Cinemachine;
using UnityEngine;

namespace Gameplay.Camera
{
    public class CameraTrackSwitch : MonoBehaviour
    {
        [SerializeField] private CinemachineVirtualCamera virtualCamera;
        [SerializeField] private CinemachinePathBase[] paths;

        private CinemachineTrackedDolly _trackedDolly;
        
        public void Switch(int index)
        {
            if (_trackedDolly == null)
            {
                _trackedDolly = virtualCamera.GetCinemachineComponent<CinemachineTrackedDolly>();
            }

            if (index > 0 && index < paths.Length)
            {
                _trackedDolly.m_Path = paths[index];
            }  
        }
    }
}