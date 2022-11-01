using System;
using UnityEngine;
using UnityEngine.Events;

namespace Common.DecisionMaking
{
    public class ActionState : MonoBehaviour, IState
    {
        [field: SerializeField] public string StateName { get; private set; }
        [field: SerializeField] public UnityEvent Entered { get; private set; }
        [field: SerializeField] public UnityEvent Exited { get; private set; }

        private readonly ActivityQueue _activityQueue = new();

        [SerializeField] private MonoActivity[] monoActivities;

        public void Enter()
        {
            Entered?.Invoke();
            foreach (var a in monoActivities)
            {
                _activityQueue.Add(a.CreateActivity());
            }
            _activityQueue.Begin();
            Debug.Log(StateName);
        }

        private void Update()
        {
            _activityQueue.Update(Time.deltaTime);
        }

        public void Exit()
        {
            Exited?.Invoke();
            _activityQueue.Clear();
        }

#if UNITY_EDITOR
        [ContextMenu("Use GameObject Name")]
        private void UseGameObjectName()
        {
            StateName = gameObject.name;
        }
#endif
    }

    public abstract class MonoActivity : MonoBehaviour
    {
        public abstract Activity CreateActivity();
    }
}