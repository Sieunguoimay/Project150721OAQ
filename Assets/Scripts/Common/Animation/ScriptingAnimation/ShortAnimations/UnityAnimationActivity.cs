using Common.Activity;
using UnityEngine;
using UnityEngine.Playables;

namespace Common.Animation.ScriptingAnimation.ShortAnimations
{
    public class UnityAnimationActivity : MonoActivity
    {
        [SerializeField] private UnityEngine.Animation anim;
        [SerializeField] private AnimationClip clip;
        private UnityEngine.Animation Animation => anim;

        public override Activity.Activity CreateActivity()
        {
            return new Lambda(OnBegin, OnUpdate);
        }

        public void Play()
        {
            if (clip == null)
            {
                Animation.Play();
            }
            else
            {
                Animation.Play(clip.name);
            }
        }
        private void OnBegin()
        {
            Play();
        }

        private bool OnUpdate()
        {
            return !Animation.isPlaying;
        }
    }
}