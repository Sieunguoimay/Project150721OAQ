using System;
using System.Linq;
using Common.UnityExtend.Attribute;
using SNM;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Events;

namespace Common.Animation
{
    public class AnimatorEventFilter : MonoBehaviour
    {
        [SerializeField] private AnimatorListener animatorListener;
#if UNITY_EDITOR
        [StringSelector(nameof(StateNames))]
#endif
        [SerializeField]
        private string stateName;

        [SerializeField] private UnityEvent onStateEnter;
        [SerializeField] private UnityEvent onStateExit;

        private int _stateHash;

#if UNITY_EDITOR
        private string[] StateNames => animatorListener.GetComponent<Animator>().GetAnimatorController().layers[0].stateMachine.states.Select(s => s.state.name).ToArray();
#endif
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