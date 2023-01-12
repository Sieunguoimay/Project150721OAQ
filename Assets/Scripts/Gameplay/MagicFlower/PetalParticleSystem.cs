using System;
using Common.Misc;
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
        [SerializeField] private UnityEvent onCollided;

        private Vector3 _position;
        private CustomConditionPooling<ParticleSystem> _pool;

        private void Start()
        {
            _pool = new CustomConditionPooling<ParticleSystem>(p => !p.isPlaying, () => Instantiate(ps, transform));
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
            onCollided?.Invoke();
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