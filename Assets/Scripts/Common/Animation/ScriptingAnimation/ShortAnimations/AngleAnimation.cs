using UnityEngine;

namespace Common.Animation.ScriptingAnimation.ShortAnimations
{
    public class AngleAnimation : Curve3FloatAnimation
    {
        protected override void OnTick(float p)
        {
            var euler = Vector3.zero;
            for (var i = 0; i < 3; i++)
            {
                euler[i] = Evaluate(i, p);
            }

            Target.localEulerAngles = euler;
        }
    }
}