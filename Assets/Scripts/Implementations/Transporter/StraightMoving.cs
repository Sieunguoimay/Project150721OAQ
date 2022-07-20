using Interfaces;
using UnityEngine;

namespace Implementations.Transporter
{
    public class StraightMoving : IMovingStyle
    {
        private IMover _mover;
        private Vector3 _origin;
        private Vector3 _target;
        private float _duration;
        private float _time;
        private bool _moving;
        private IMovingStyleHandler _handler;

        public void StartMoving(Vector3 target, float speed)
        {
            if (_mover == null) return;
            _origin = _mover.GetPosition();
            _target = target;
            _duration = (_target - _origin).magnitude / speed;
            _time = 0f;
            _moving = true;
            
        }

        public void SetMover(IMover mover)
        {
            _mover = mover;
        }

        public void SetMovingStyleHandler(IMovingStyleHandler handler)
        {
            _handler = handler;
        }

        public void Loop(float deltaTime)
        {
            if (_moving)
            {
                if (_time < _duration)
                {
                    _time += deltaTime;
                    var t = _time / _duration;
                    _mover.SetPosition(Vector3.Lerp(_origin, _target, t));
                }
                else
                {
                    _moving = false;
                    _mover.SetPosition(_target);
                    _handler?.OnMovingStyleResult();
                }
            }
        }
    }
}