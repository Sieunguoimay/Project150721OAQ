using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;

namespace Common.Animation
{
    public class AnimationStateEvent : StateMachineBehaviour
    {
        private AnimatorListener _listener;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex, controller);
            GetListener(animator).OnStateEnter(stateInfo, layerIndex);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
        {
            base.OnStateExit(animator, stateInfo, layerIndex, controller);
            GetListener(animator).OnStateExit(stateInfo, layerIndex);
        }

        private AnimatorListener GetListener(Animator animator)
        {
            if (_listener != null) return _listener;
            
            _listener = animator.GetComponentInChildren<AnimatorListener>();
            _listener.Setup(animator);

            return _listener;
        }
    }
}