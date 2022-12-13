using System;
using UnityEngine;
using UnityEngine.Events;

namespace Common.DecisionMaking.Actions
{
    public class MonoActivityTimer : MonoActivity
    {
        [SerializeField] private float duration;
        [SerializeField] private UnityEventFloat onProgress;

        private Activity _activity;

        public override Activity CreateActivity()
        {
            if (_activity == null)
            {
                _activity = new ActivityTimer(duration, p => onProgress?.Invoke(p),true);
            }

            return _activity;
        }

        public void SetDuration(float d)
        {
            Debug.Log($"SetDuration {d}");
            _activity = null;
            duration = d;
        }

        [Serializable]
        public class UnityEventFloat : UnityEvent<float>
        {
        }
    }
}