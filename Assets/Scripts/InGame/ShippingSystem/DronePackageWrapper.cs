using UnityEngine;

namespace InGame.ShippingSystem
{
    public class DronePackageWrapper : IDronePackage
    {
        private readonly Transform _transform;
        private readonly Vector3 _offset;

        public DronePackageWrapper(Transform transform, Vector3 offset)
        {
            _transform = transform;
            _offset = offset;
        }

        public Transform GetTransform()
        {
            return _transform;
        }

        public Vector3 GetPickupPoint()
        {
            return _offset;
        }
    }
}