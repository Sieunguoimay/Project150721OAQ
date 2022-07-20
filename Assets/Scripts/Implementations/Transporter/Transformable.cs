using Interfaces;
using UnityEngine;

namespace Implementations.Transporter
{
    public class Transformable : ITransformable
    {
        private readonly Transform _transform;

        public Transformable(Transform transform)
        {
            _transform = transform;
        }

        public Vector3 GetPosition()
        {
            return _transform.position;
        }

        public void SetPosition(Vector3 position)
        {
            _transform.position = position;
        }

        public void SetRotation(Quaternion rotation)
        {
            _transform.rotation = rotation;
        }

        public Quaternion GetRotation()
        {
            return _transform.rotation;
        }
    }
}