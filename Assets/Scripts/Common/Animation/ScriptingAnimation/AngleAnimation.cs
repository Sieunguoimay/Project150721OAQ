using UnityEngine;

namespace Common.Animation.ScriptingAnimation
{
    public class AngleAnimation : ScriptingAnimation
    {
        [SerializeField] private AnimationCurve curveX;
        [SerializeField] private AnimationCurve curveY;
        [SerializeField] private AnimationCurve curveZ;

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