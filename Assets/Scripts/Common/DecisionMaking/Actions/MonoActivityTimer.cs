using System;
using Common.Activity;
using UnityEngine;
using UnityEngine.Events;

namespace Common.DecisionMaking.Actions
{
    public class MonoActivityTimer : MonoActivity
    {
        [SerializeField] private float duration;
        [SerializeField] private UnityEventFloat onProgress;

        private Activity.Activity _activity;

        public override Activity.Activity CreateActivity()
        {
            return _activity ??= new ActivityTimer(duration, p => onProgress?.Invoke(p), true);
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