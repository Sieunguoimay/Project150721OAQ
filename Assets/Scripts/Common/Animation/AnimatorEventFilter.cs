using System;
using Common.ResolveSystem;
using UnityEngine;
using UnityEngine.Events;

namespace Common.Animation
{
    public class AnimatorEventFilter : MonoBehaviour
    {
        [SerializeField] private AnimatorListener animatorListener;
        [SerializeField] private string stateName;
        [SerializeField] private UnityEvent onStateEnter;
        [SerializeField] private UnityEvent onStateExit;

        private int _stateHash;

        private void OnEnable()
        {
            animatorListener.StateEnter += OnStateEnter;
            animatorListener.StateExit += OnStateExit;
            _stateHash = Animator.StringToHash(stateName);
        }

        private void OnDisable()
        {
            animatorListener.StateEnter -= OnStateEnter;
            animatorListener.StateExit -= OnStateExit;
        }

        private void OnStateEnter(AnimatorStateInfo arg1, int arg2)
        {
            if (arg1.shortNameHash == _stateHash)
            {
                onStateEnter?.Invoke();
            }
        }

        private void OnStateExit(AnimatorStateInfo arg1, int arg2)
        {
            if (arg1.shortNameHash == _stateHash)
            {
                onStateExit?.Invoke();
            }
        }
    }
}