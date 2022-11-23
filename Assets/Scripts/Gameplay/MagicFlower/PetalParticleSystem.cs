using System;
using Common.UnityExtend;
using Common.UnityExtend.ParticleSystem;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace Gameplay.MagicFlower
{
    public class PetalParticleSystem : MonoBehaviour
    {
        [SerializeField] private ParticleSystem ps;
        [SerializeField] private ParticleSystem collisionPs;

        private Vector3 _position;
        private UnityObjectPooling<ParticleSystem> _pool;

        private void Start()
        {
            _pool = new UnityObjectPooling<ParticleSystem>(transform, ps, p => !p.isPlaying);
        }

        private void OnDestroy()
        {
            foreach (var p in _pool.Items)
            {
                var collidedEvent = p.GetComponent<ParticleSystemCollide>()?.Collided;
                collidedEvent?.RemoveListener(OnCollided);
            }
        }

        private void OnCollided(ParticleCollisionEvent arg0)
        {
            collisionPs.transform.position = arg0.intersection;
            collisionPs.Emit(Random.Range(2, 3));
        }

        public void Emit(int count)
        {
            var p = _pool.GetFromPool();

            var collidedEvent = p.GetComponent<ParticleSystemCollide>()?.Collided;
            collidedEvent?.RemoveListener(OnCollided);
            collidedEvent?.AddListener(OnCollided);

            p.transform.position = _position;
            p.Emit(count);
        }

        public void SetEmitPosition(Vector3 pos)
        {
            _position = pos;
        }
    }
}