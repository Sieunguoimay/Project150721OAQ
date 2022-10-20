using System;
using System.Collections.Generic;
using Common;
using CommonActivities;
using Gameplay.Piece;
using UnityEngine;

namespace Gameplay
{
    public class Flocking : Activity
    {
        protected MotionMetrics Motion;

        private readonly ConfigData _configData;
        protected Vector3 Target;
        protected readonly Transform Transform;
        private Flocking[] _others;

        public Flocking(ConfigData configData, Vector3 target, Transform transform, Flocking[] others)
        {
            _others = others ?? new Flocking[0];
            _configData = configData;
            Target = target;
            Transform = transform;
            Motion = new MotionMetrics();
        }

        public override bool Inactive => !Motion.moving;

        public override void Begin()
        {
            Motion.position = Transform.position;
            Motion.moving = true;
            Motion.direction = Transform.forward;
        }

        public override void Update(float deltaTime)
        {
            if (!Motion.moving) return;

            if (CheckDistance(Motion, _configData.arriveDistance))
            {
                Motion.acceleration += Arrive(Motion, Target, deltaTime);
            }
            else
            {
                Motion.acceleration += Seek(Motion, Target);
                Motion.acceleration += Separate(_others);
            }

            SetPosAndForward(Motion.GetFinalPosition(deltaTime, _configData), Motion.direction);

            Motion.acceleration = Vector3.zero;

            if (CheckDistance(Motion, 0.001f))
            {
                CancelMove();
            }
        }

        public override void End()
        {
        }

        public void SetOthers(Flocking[] others)
        {
            _others = others;
        }

        public void AddVelocity(Vector3 velocity)
        {
            Motion.velocity += velocity;
        }

        protected virtual void SetPosAndForward(Vector3 pos, Vector3 forward)
        {
            Transform.position = pos;
            Transform.forward = forward;
        }

        private bool CheckDistance(MotionMetrics motionMetrics, float distance)
        {
            return (motionMetrics.position - Target).sqrMagnitude < distance * distance;
        }

        private void CancelMove()
        {
            Motion.velocity = Vector3.zero;
            Motion.position = Target;
            Motion.moving = false;
        }

        protected float Speed => _configData.maxSpeed;

        private Vector3 Arrive(MotionMetrics motion, Vector3 target, float deltaTime)
        {
            var diff = target - motion.position;
            var mag = diff.magnitude;
            var f = mag / _configData.arriveDistance;
            var desiredVelocity = (diff / mag) * Mathf.Lerp(0f, _configData.maxSpeed, 1 - (f - 1) * (f - 1));

            var desiredAcceleration = (desiredVelocity - motion.velocity) / deltaTime;

            return desiredAcceleration;
        }

        private Vector3 Seek(MotionMetrics motion, Vector3 target)
        {
            var diff = target - motion.position;
            var mag = diff.magnitude;
            var desiredVelocity = diff / mag;
            desiredVelocity *= _configData.maxSpeed;

            var desiredAcceleration = desiredVelocity - motion.velocity;
            desiredAcceleration = SNM.Math.ClampMagnitude(desiredAcceleration, _configData.maxAcceleration);

            return desiredAcceleration;
        }

        private Vector3 Separate(IEnumerable<Flocking> others)
        {
            var desiredSeparation = _configData.spacing;
            var sum = new Vector3();
            var count = 0;
            foreach (var other in others)
            {
                var d = Vector3.Distance(Motion.position, other.Motion.position);
                if ((!(d > 0)) || (!(d < desiredSeparation))) continue;

                var diff = Motion.position - other.Motion.position;
                diff = diff.normalized;
                diff /= d;
                sum += diff;
                count++;
            }

            if (count <= 0) return Vector3.zero;

            sum /= count;
            sum = sum.normalized;
            sum *= _configData.maxSpeed;

            var desiredAcceleration = sum - Motion.velocity;
            desiredAcceleration = SNM.Math.ClampMagnitude(desiredAcceleration, _configData.maxAcceleration);

            return desiredAcceleration;
        }

        protected struct MotionMetrics
        {
            public bool moving;

            public Vector3 position;
            public Vector3 velocity;
            public Vector3 acceleration;
            public Vector3 direction;

            public Vector3 GetFinalPosition(float deltaTime, Flocking.ConfigData config)
            {
                velocity += acceleration * deltaTime;
                velocity = SNM.Math.ClampMagnitude(velocity, config.maxSpeed);
                position += velocity * deltaTime;
                if (velocity.magnitude > 0.0001f)
                {
                    direction = velocity.normalized;
                }

                return position;
            }
        }


        [Serializable]
        public struct ConfigData
        {
            public float maxSpeed;
            public float maxAcceleration;
            public float arriveDistance;
            public float spacing;
        }
    }

    public class JumpingFlocking : Flocking
    {
        private Vector3 _flockingPosition;
        private readonly Gameplay.Piece.Jump _jump;
        private bool _delay;
        private float _intervalTime;
        private bool _noJumping;

        public override bool Inactive => base.Inactive && _jump.Inactive;

        public JumpingFlocking(ConfigData configData, Vector3 target, Transform transform, Flocking[] others)
            : base(configData, target, transform, others)
        {
            _jump = new Gameplay.Piece.Jump(Transform, new Gameplay.Piece.Jump.InputData
            {
                Height = 0.3f,
                Duration = 0.25f
            }, new LinearEasing());
        }

        protected override void SetPosAndForward(Vector3 pos, Vector3 forward)
        {
            _flockingPosition = pos;
            Transform.forward = forward;

            var p = Transform.position;
            Transform.position = new Vector3(_flockingPosition.x, p.y, _flockingPosition.z);
        }

        public override void Begin()
        {
            base.Begin();
            _jump.Begin();
            _noJumping = false;
        }

        public override void Update(float deltaTime)
        {
            _jump.Update(deltaTime);
            if (Motion.moving)
            {
                if (!_noJumping && _jump.Inactive)
                {
                    _delay = true;
                }
                else
                {
                    base.Update(deltaTime);
                }
            }

            if (_noJumping || !_delay) return;

            _intervalTime += deltaTime;
            if (_intervalTime < 0.08f) return;
            _intervalTime = 0f;

            var jumpDistance = _jump.GetJumpDistance(Speed);
            if (Vector3.SqrMagnitude(Target - Motion.position) > jumpDistance * jumpDistance)
            {
                _delay = false;
                _jump.Begin();
            }
            else
            {
                _noJumping = true;
            }
        }
    }
}