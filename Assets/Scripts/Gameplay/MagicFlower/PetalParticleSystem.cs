using Common.UnityExtend;
using Common.UnityExtend.ParticleSystem;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.MagicFlower
{

    public class PetalParticleSystem : MonoBehaviour
    {
        [SerializeField] private ParticleSystem ps;

        public UnityEvent<ParticleCollisionEvent> Collided => ps.GetComponent<ParticleSystemCollide>()?.Collided;

        // private ParticleSystemForceField _forceField;

        private Vector3 _position;
        private UnityObjectPooling<ParticleSystem> _pool;

        private void Start()
        {
            // _forceField = ps.externalForces.GetInfluence(0);
            _pool = new UnityObjectPooling<ParticleSystem>(transform, ps, p => !p.isPlaying);
        }

        public void Emit(int count)
        {
            var p = _pool.GetFromPool();
            p.transform.position = _position;
            p.Emit(count);
        }

        public void SetEmitPosition(Vector3 pos)
        {
            _position = pos;
        }

        // public void SetColliderPosition(Vector3 pos)
        // {
        //     var forceField = ps.externalForces.GetInfluence(0);
        //     forceField.transform.position = pos;
        //     UpdateForceField();
        // }

        // public void UpdateForceField()
        // {
        //     var forceFieldTransform = _forceField.transform;
        //     var diff = ps.transform.position - forceFieldTransform.position;
        //     var plane = ps.collision.GetPlane(0);
        //     plane.up = diff.normalized;
        //     _forceField.endRange = diff.magnitude + 5f;
        // }
    }
}