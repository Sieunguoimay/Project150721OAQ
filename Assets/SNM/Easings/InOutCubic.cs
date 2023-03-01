using Common;
using UnityEngine;

namespace SNM.Easings
{
    public class InOutCubic : IEasing
    {
        public virtual float GetEase(float x)
        {
            return x < 0.5f ? 4f * x * x * x : 1f - Mathf.Pow(-2f * x + 2f, 3f) / 2f;
        }
    }

    public class InOutCubicInversed : InOutCubic
    {
        public override float GetEase(float x)
        {
            return 1f/base.GetEase(x);
        }
    }
}