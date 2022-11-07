using UnityEngine;

namespace Common.DecisionMaking.Actions
{
    public class MonoActivityTimer : MonoActivity
    {
        [SerializeField] private float duration;

        private Activity _activity;

        public override Activity CreateActivity()
        {
            if (_activity == null)
            {
                _activity = new ActivityTimer(duration, null);
            }

            return _activity;
        }

        public void SetDuration(float d) => duration = d;
    }
}