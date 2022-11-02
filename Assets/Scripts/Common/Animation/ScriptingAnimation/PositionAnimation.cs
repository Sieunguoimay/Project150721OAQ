using UnityEngine;

namespace Common.Animation.ScriptingAnimation
{
    public class PositionAnimation:ScriptingAnimation
    {
        [SerializeField] private AnimationCurve curveX;
        [SerializeField] private AnimationCurve curveY;
        [SerializeField] private AnimationCurve curveZ;

        protected override void OnTick(float p)
        {
            var pos = Target.localPosition;
            pos.x = curveX.Evaluate(p);
            pos.y = curveY.Evaluate(p);
            pos.z = curveZ.Evaluate(p);
            Target.localPosition = pos;
        }
    }
}