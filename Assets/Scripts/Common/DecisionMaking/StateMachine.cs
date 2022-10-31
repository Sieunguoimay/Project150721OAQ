using UnityEngine;

namespace Common.DecisionMaking
{
    public class StateMachine
    {
        private IState[] _states;
        private int _currentState = -1;

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

            var prevState = _currentState;
            _currentState = stateIndex;
            if (prevState >= 0)
            {
                _states[prevState].Exit();
            }

            _states[_currentState].Enter();
        }
    }

    public interface IState
    {
        void Enter();
        void Exit();
    }
}