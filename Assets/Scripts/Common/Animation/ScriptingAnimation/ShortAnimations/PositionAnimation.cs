using System;
using Common.UnityExtend.Attribute;
using UnityEngine;

namespace Common.Animation.ScriptingAnimation.ShortAnimations
{
    public abstract class Curve3FloatAnimation : ScriptingAnimation
    {
        [SerializeField] protected AnimationCurve[] curves;
        public AnimationCurve GetCurve(int index) => curves[index];

        protected float Evaluate(int index, float value)
        {
            index = Mathf.Min(index, curves.Length - 1);
            return curves[index].Evaluate(value);
        }

#if UNITY_EDITOR
        [SerializeField] private bool test;

        [SerializeField, Range(0, 1)]
        private float testLerp;

        private void OnValidate()
        {
            if (test)
            {
                OnTick(testLerp);
            }
        }
#endif
    }

    public class PositionAnimation : Curve3FloatAnimation
    {
        protected override void OnTick(float p)
        {
            var pos = Vector3.zero;
            for (var i = 0; i < 3; i++)
            {
                pos[i] = Evaluate(i, p);
            }

            Target.localPosition = pos;
        }
    }
}