using System;
using System.Collections.Generic;
using UnityEngine;

namespace Common.DecisionMaking
{
    public interface IStateMachine
    {
        IReadOnlyList<IState> States { get; }
        IState CurrentState { get; }
        void SetStates(IState[] states, int defaultStateIndex = 0);
        void ChangeState(IState state);
    }

    public class StateMachine : IStateMachine
    {
        private IState[] _states;

        public IReadOnlyList<IState> States => _states;
        public IState CurrentState { get; private set; }

        public void SetStates(IState[] states, int defaultState = 0)
        {
            _states = states;
            ChangeState(_states[defaultState]);
        }

        public void ChangeState(IState state)
        {
            if (Array.IndexOf(_states, state) == -1)
            {
                Debug.LogError($"Wrong state {state}");
                return;
            }
            CurrentState?.Exit();
            CurrentState = state;
            CurrentState.Enter();
        }
    }

    public interface IState
    {
        void Enter();
        void Exit();
    }

    public abstract class BaseState : IState
    {
        public IStateMachine StateMachine { get; }

        protected BaseState(IStateMachine stateMachine)
        {
            StateMachine = stateMachine;
        }

        protected abstract void OnEnter();
        protected abstract void OnExit();

        public void Enter()
        {
            OnEnter();
        }

        public void Exit()
        {
            OnExit();
        }
    }
}