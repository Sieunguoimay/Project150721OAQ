using UnityEngine;

namespace Common.Animation.ScriptingAnimation
{
    public class AngleAnimation : ScriptingAnimation
    {
        public AnimationCurve curveX;
        public AnimationCurve curveY;
        public AnimationCurve curveZ;

        protected override void OnTick(float p)
        {
            var euler = Target.localEulerAngles;
            euler.x = curveX.Evaluate(p);
            euler.y = curveY.Evaluate(p);
            euler.z = curveZ.Evaluate(p);
            Target.localEulerAngles = euler;
        }
    }
}