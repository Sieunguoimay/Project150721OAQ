using System;
using Common;
using Gameplay.Piece.Activities;
using UnityEngine;
using UnityEngine.Playables;

namespace Gameplay.Piece
{
    public class Piece : MonoBehaviour
    {
        [SerializeField] private Vector3 size;
        [SerializeField] private ActivityFlocking.ConfigData flockingConfigData = new()
        {
            maxSpeed = 3f,
            maxAcceleration = 10f,
            arriveDistance = 1f,
            spacing = 0.3f
        };

        [field: NonSerialized] public ActivityQueue ActivityQueue { get; } = new();
        public virtual Animator Animator => null;
        public virtual PlayableDirector JumpTimeline => null;
        public ActivityFlocking.ConfigData FlockingConfigData => flockingConfigData;

        private void Update()
        {
            ActivityQueue?.Update(Time.deltaTime);
        }


#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            var t = transform;
            Gizmos.DrawWireCube(t.position + t.up * size.y * 0.5f, size);
        }
#endif
    }
}