using System;
using Common.DecisionMaking;
using UnityEngine;

namespace Common.Activity
{
    public abstract class MonoActivity : MonoBehaviour
    {
        public abstract Activity CreateActivity();
    }

    public class MonoActivityQueue : MonoActivity
    {
        [SerializeField] private MonoActivity[] monoActivities;

        private readonly ActivityQueue _activityQueue = new();

        public void Begin()
        {
            if (!_activityQueue.Inactive)
            {
                _activityQueue.End();
            }

            CreateActivity();

            _activityQueue.Begin();
        }

        public void End()
        {
            _activityQueue.End();
        }

        private void Update()
        {
            _activityQueue.Update(Time.deltaTime);
        }

        public override Activity CreateActivity()
        {
            foreach (var a in monoActivities)
            {
                _activityQueue.Add(a.CreateActivity());
            }

            return _activityQueue;
        }
    }
}