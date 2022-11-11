using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Common.UnityExtend.ParticleSystem
{
    [RequireComponent(typeof(UnityEngine.ParticleSystem))]
    public class ParticleSystemCollide : MonoBehaviour
    {
        [field: SerializeField] public UnityEvent<ParticleCollisionEvent> Collided { get; private set; }
        [field: NonSerialized] public UnityEngine.ParticleSystem ParticleSystem { get; private set; }

        private List<ParticleCollisionEvent> _collisionEvents;

        private UnityEngine.ParticleSystem.Particle[] _particle;

        private void Start()
        {
            _collisionEvents = new List<ParticleCollisionEvent>();
            ParticleSystem = GetComponent<UnityEngine.ParticleSystem>();
        }

        private void OnParticleCollision(GameObject other)
        {
            var numCollisionEvents = ParticleSystem.GetCollisionEvents(other, _collisionEvents);

            var i = 0;
            while (i < numCollisionEvents)
            {
                Collided?.Invoke(_collisionEvents[i]);
                i++;
            }
        }
    }
}