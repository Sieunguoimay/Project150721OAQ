using System;
using Common.ResolveSystem;
using UnityEditor.Animations;
using UnityEngine;

namespace Common.Animation
{
    public sealed class AnimatorListener : MonoBehaviour
    {
        public event Action<AnimatorStateInfo, int> StateEnter;
        public event Action<AnimatorStateInfo, int> StateExit;
        
        public Animator Animator { get; private set; }

        public void Setup(Animator animator)
        {
            Animator = animator;
        }

        public void OnStateEnter(AnimatorStateInfo arg1, int arg2)
        {
            StateEnter?.Invoke(arg1, arg2);
        }

        public void OnStateExit(AnimatorStateInfo arg1, int arg2)
        {
            StateExit?.Invoke(arg1, arg2);
        }
    }
}