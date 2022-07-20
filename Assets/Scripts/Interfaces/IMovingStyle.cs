using UnityEngine;

namespace Interfaces
{
    public interface IMovingStyle
    {
        void StartMoving(Vector3 target, float speed);
        void SetMover(IMover mover);
        void SetMovingStyleHandler(IMovingStyleHandler handler);
        void Loop(float deltaTime);
    }

    public interface IMovingStyleHandler
    {
        void OnMovingStyleResult();
    }
}