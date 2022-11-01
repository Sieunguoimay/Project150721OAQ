using System;
using UnityEngine;

namespace Common.Misc
{
    public class FaceCamera : MonoBehaviour
    {
        public Transform target;
        private Camera _cam;

        private void Start()
        {
            _cam = Camera.main;
        }

        public void FaceTowardCamera()
        {
            var t = target ? target : transform;
            t.rotation = Quaternion.LookRotation(_cam.transform.position - t.position);
        }
    }
}