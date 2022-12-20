using UnityEngine;

namespace Common.Animation.ScriptingAnimation.ShortAnimations
{
    public class ScaleAnimation : Curve3FloatAnimation
    {
        [SerializeField] private bool multiply;
        private Vector3 _initialValue;

        public override void Play()
        {
            if (!ActivityQueue.Inactive) return;
            base.Play();
            _initialValue = Target.localScale;
        }

        protected override void OnTick(float p)
        {
            var scale = Vector3.zero;
            for (var i = 0; i < 3; i++)
            {
                scale[i] = (multiply ? _initialValue[i] : 1f) * Evaluate(i, p);
            }

            Target.localScale = scale;
        }
    }
}