using UnityEngine;

namespace Common.DecisionMaking
{
    public class StateTransition : MonoBehaviour
    {
        [field: SerializeField] public string TargetState { get; private set; }
        private StateMachine _stateMachine;
        private int _index;

        public void Setup(StateMachine stateMachine, int index)
        {
            _stateMachine = stateMachine;
            _index = index;
        }

        public void Transition()
        {
            _stateMachine.ChangeState(_index);
        }
    }
}