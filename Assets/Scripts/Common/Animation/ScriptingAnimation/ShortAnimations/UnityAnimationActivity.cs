using Common.Activity;
using UnityEngine;
using UnityEngine.Playables;

namespace Common.Animation.ScriptingAnimation.ShortAnimations
{
    public class UnityAnimationActivity : MonoActivity
    {
        [SerializeField] private UnityEngine.Animation anim;
        [SerializeField] private string animationName;
        private UnityEngine.Animation Animation => anim;

        public override Activity.Activity CreateActivity()
        {
            return new Lambda(OnBegin, OnUpdate);
        }

        private void OnBegin()
        {
            if (string.IsNullOrEmpty(animationName))
            {
                Animation.Play();
            }
            else
            {
                Animation.Play(animationName);
            }
        }

        private bool OnUpdate()
        {
            return !Animation.isPlaying;
        }
    }
}