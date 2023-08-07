﻿using System;
using UnityEngine;

namespace Common.Animation
{
    public sealed class AnimatorStateEventListener : MonoBehaviour, IAnimationStateEventHandler
    {
        public event Action<AnimatorStateInfo, int> StateEnter;
        public event Action<AnimatorStateInfo, int> StateExit;

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