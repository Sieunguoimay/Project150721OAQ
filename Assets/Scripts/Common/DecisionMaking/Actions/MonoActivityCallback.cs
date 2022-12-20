using Common.Activity;
using Gameplay.Piece.Activities;
using UnityEngine;
using UnityEngine.Events;

namespace Common.DecisionMaking.Actions
{
    public class MonoActivityCallback : MonoActivity
    {
        [SerializeField] private UnityEvent callback;

        public override Activity.Activity CreateActivity()
        {
            return new ActivityCallback(() => callback?.Invoke());
        }
    }
}