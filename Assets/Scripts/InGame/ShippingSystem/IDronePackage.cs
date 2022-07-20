using UnityEngine;

namespace InGame.ShippingSystem
{
    public interface IDronePackage
    {
        Transform GetTransform();
        Vector3 GetPickupPoint();
    }
}