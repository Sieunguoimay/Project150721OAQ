using UnityEngine;

namespace Common.Animation.ScriptingAnimation
{
    public class PositionAnimation:ScriptingAnimation
    {
        [SerializeField] private AnimationCurve[] curves;

        protected override void OnTick(float p)
        {
            var pos = Target.localPosition;
            for (var i = 0; i < 3; i++)
            {
                pos[i] = curves[i].Evaluate(p);
            }
            Target.localPosition = pos;
        }
    }
}