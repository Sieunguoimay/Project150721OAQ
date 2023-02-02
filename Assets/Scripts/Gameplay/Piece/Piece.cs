using Common.Activity;
using Gameplay.Piece.Activities;
using UnityEngine;

namespace Gameplay.Piece
{
    public interface IPiece
    {
        Transform Transform { get; }
        IActivityQueue ActivityQueue { get; }
    }

    public class Piece : MonoBehaviour, IPiece
    {
        [SerializeField] private Vector3 size;

        [SerializeField] private ActivityFlocking.ConfigData flockingConfigData = new()
        {
            maxSpeed = 3f,
            maxAcceleration = 10f,
            arriveDistance = 1f,
            spacing = 0.3f
        };


        private readonly ActivityQueue _activityQueue = new();
        public IActivityQueue ActivityQueue => _activityQueue;
        // public virtual Animator Animator => null;
        // public virtual PlayableDirector JumpTimeline => null;
        public ActivityFlocking.ConfigData FlockingConfigData => flockingConfigData;
        public Transform Transform => transform;

        private void Update()
        {
            _activityQueue.Update(Time.deltaTime);
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