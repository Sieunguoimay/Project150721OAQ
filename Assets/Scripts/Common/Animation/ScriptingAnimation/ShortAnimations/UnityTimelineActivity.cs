using System;
using Common.Activity;
using UnityEngine;
using UnityEngine.Playables;

namespace Common.Animation.ScriptingAnimation.ShortAnimations
{
    public class UnityTimelineActivity : MonoActivity
    {
        private PlayableDirector _animation;
        private PlayableDirector Animation => _animation ??= GetComponent<PlayableDirector>();

        public override Activity.Activity CreateActivity()
        {
            return new InnerActivity(Animation);
        }

        private class InnerActivity : Activity.Activity
        {
            private readonly PlayableDirector _animation;

            public InnerActivity(PlayableDirector playableDirector)
            {
                _animation = playableDirector;
            }

            public override void Begin()
            {
                base.Begin();
                _animation.Play();
                _animation.stopped += OnStopped;
            }

            private void OnStopped(PlayableDirector obj)
            {
                End();
            }
        }
    }
}