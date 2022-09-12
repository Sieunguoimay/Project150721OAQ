using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Gameplay.Piece
{
    public class Piece : MonoBehaviour
    {
        [SerializeField] private ConfigData config;

        [Serializable]
        public class ConfigData
        {
            public int point;
            public Vector3 size;

            public Flocking.ConfigData flockingConfigData = new()
            {
                maxSpeed = 3f,
                maxAcceleration = 10f,
                arriveDistance = 1f,
                spacing = 0.3f
            };
        }

        [field: NonSerialized] public PieceActivityQueue PieceActivityQueue { get; } = new();
        public ConfigData Config => config;
        public virtual Animator Animator => null;
        public virtual PlayableDirector JumpTimeline => null;

        public virtual void Setup()
        {
        }

        private void Update()
        {
            PieceActivityQueue?.Update(Time.deltaTime);
        }



#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            if (Config == null) return;

            var t = transform;
            Gizmos.DrawWireCube(t.position + t.up * Config.size.y * 0.5f, Config.size);
        }
#endif
    }
}