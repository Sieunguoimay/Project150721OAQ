using System.Collections.Generic;
using UnityEngine;

namespace Common.Animation.ScriptingAnimation
{
    public class SwayAnimation : ScriptingAnimation
    {
        [SerializeField, Min(0)] private int cycle = 1;
        [SerializeField, Range(0f, 180f)] private float amplitude = 20f;
        [SerializeField, Range(-180, 180)] private float angle;

        private readonly List<Transform> _hierarchy = new();
        private readonly List<Quaternion> _defaultQuaternions = new();

        private void Start()
        {
            CacheHierarchy();
        }

        protected override void OnTick(float p)
        {
            // Debug.Log(angle+" "+p);
            UpdateSway(p);
        }

        public void RandomizeSwayDirection()
        {
            angle = UnityEngine.Random.Range(-180f, 180f);
            amplitude = Random.Range(2, 5);
        }

        public void UpdateSway(float p)
        {
            var horizontalDirection = Quaternion.Euler(Vector3.up * angle) * Vector3.forward;

            if (_hierarchy.Count == 0) return;
            var amp = Mathf.Lerp(amplitude, 0f, p);
            for (var i = 0; i < _hierarchy.Count; i++)
            {
                var sin = amp * Mathf.Sin(p * Mathf.PI * 2f * cycle);

                var euler = horizontalDirection * sin;
                var quaternion = Quaternion.Euler(euler);

                _hierarchy[i].localRotation = _defaultQuaternions[i] * quaternion;
            }
        }

        public void CacheHierarchy()
        {
            var current = Target;
            while (current.childCount > 0)
            {
                var child = current.GetChild(0);
                _hierarchy.Add(child);
                _defaultQuaternions.Add(child.localRotation);
                current = child;
            }
        }
    }
}