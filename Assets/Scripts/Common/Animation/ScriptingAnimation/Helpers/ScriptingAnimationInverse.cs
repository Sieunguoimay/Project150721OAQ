using Common.Activity;
using Common.DecisionMaking;
using UnityEngine;

namespace Common.Animation.ScriptingAnimation.Helpers
{
    public sealed class ScriptingAnimationInverse : MonoActivity
    {
        [SerializeField] private ScriptingAnimation scriptingAnimation;

        [ContextMenu("Play")]
        public void Play()
        {
            scriptingAnimation.ActivityQueue.End();
            scriptingAnimation.ActivityQueue.Add(new ActivityTimer(scriptingAnimation.Duration, Tick, true));
            scriptingAnimation.ActivityQueue.Begin();
        }

        public override Activity.Activity CreateActivity()
        {
            return new ActivityTimer(scriptingAnimation.Duration, Tick, true);
        }

        private void Tick(float p)
        {
            scriptingAnimation.Tick(1 - p);
        }
    }
}