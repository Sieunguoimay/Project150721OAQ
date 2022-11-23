using UnityEngine;

namespace Common.DecisionMaking
{
    public class StateMachine
    {
        private IState[] _states;

        public int CurrentState { get; private set; } = -1;

        public void SetStates(IState[] states, int defaultState = 0)
        {
            _states = states;
            ChangeState(defaultState);
        }

        public void ChangeState(int stateIndex)
        {
            if (stateIndex >= _states.Length || stateIndex < 0)
            {
                Debug.LogError($"Wrong state index {stateIndex}");
                return;
            }

            var prevState = CurrentState;
            CurrentState = stateIndex;
            if (prevState >= 0)
            {
                _states[prevState].Exit();
            }

            _states[CurrentState].Enter();
        }
    }

    public interface IState
    {
        void Enter();
        void Exit();
    }
}