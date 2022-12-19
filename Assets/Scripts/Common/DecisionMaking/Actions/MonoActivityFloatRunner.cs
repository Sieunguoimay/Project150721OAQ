using UnityEngine;

namespace Common.DecisionMaking.Actions
{
    public class MonoActivityFloatRunner : MonoActivity
    {
        [SerializeField] private float beginValue = 0;
        [SerializeField] private float endValue = 1;
        [SerializeField] private float speed = 1;
        [SerializeField] private FloatRunnerActivity.LoopType loopType = FloatRunnerActivity.LoopType.None;
        [SerializeField] private MonoActivityTimer.UnityEventFloat onProgress;

        public override Activity CreateActivity()
        {
            return new FloatRunnerActivity(beginValue, endValue, speed, loopType, OnProgress);
        }

        private void OnProgress(float value)
        {
            onProgress?.Invoke(value);
        }
    }
}