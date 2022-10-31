using UnityEngine;
using UnityEngine.Events;

namespace Common.DecisionMaking
{
    public class ActionState : MonoBehaviour, IState
    {
        [field:SerializeField] public UnityEvent Entered { get; private set; }
        [field:SerializeField] public UnityEvent Exited { get; private set; }
        public void Enter()
        {
            Entered?.Invoke();
        }

        public void Exit()
        {
            Exited?.Invoke();
        }
    }
}