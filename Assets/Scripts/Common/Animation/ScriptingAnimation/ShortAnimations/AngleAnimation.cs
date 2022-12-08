using UnityEngine;

namespace Common.Animation.ScriptingAnimation.ShortAnimations
{
    public class AngleAnimation : ScriptingAnimation
    {
        public AnimationCurve[] curves;

        protected override void OnTick(float p)
        {
            var euler = Target.localEulerAngles;
            for (var i = 0; i < 3; i++)
            {
                euler[i] = curves[i].Evaluate(p);
            }

            Target.localEulerAngles = euler;
        }

    }
}