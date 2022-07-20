using UnityEngine;

namespace Interfaces
{
    public interface IMover : ITransformable, IItemHolder<IMoverListener>
    {
        void MoveTo(Vector3 position);
        void Loop(float deltaTime);
        void SetMovingStyle(IMovingStyle style);
    }

    public interface IMoverListener
    {
        void OnReachTarget(IMover mover);
    }
}