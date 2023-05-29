using System;
using UnityEngine;

namespace Common.Animation
{
    public class AnimationStateEvent : StateMachineBehaviour
    {
        private IAnimationStateEventHandler[] _listeners;

        public override void OnStateEnter(UnityEngine.Animator animator, UnityEngine.AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            TryGetListeners(animator);

            if (_listeners == null) return;
            for (var i = 0; i < _listeners.Length; i++)
            {
                _listeners[i].OnStateEnter(stateInfo, layerIndex);
            }
        }

        public override void OnStateExit(UnityEngine.Animator animator, UnityEngine.AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);

            TryGetListeners(animator);

            if (_listeners == null) return;
            for (var i = 0; i < _listeners.Length; i++)
            {
                _listeners[i].OnStateExit(stateInfo, layerIndex);
            }
        }

        private void TryGetListeners(Animator animator)
        {
            if (_listeners != null) return;
            _listeners = animator.GetComponentsInChildren<IAnimationStateEventHandler>();
        }
    }

    public interface IAnimationStateEventHandler
    {
        void OnStateEnter(AnimatorStateInfo stateInfo, int layerIndex);
        void OnStateExit(AnimatorStateInfo stateInfo, int layerIndex);
    }
}