using UnityEngine;

namespace Interfaces
{
    public interface ITransformable : ILocatable
    {
        void SetPosition(Vector3 position);
        void SetRotation(Quaternion rotation);
        Quaternion GetRotation();
    }
}