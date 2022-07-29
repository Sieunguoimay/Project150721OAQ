using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Boid : SNM.Activity
{
    protected MotionMetrics motion;

    private readonly ConfigData _configData;
    protected InputData inputData;
    private Boid[] _others;

    public Boid(ConfigData configData, InputData inputData, Boid[] others)
    {
        this._others = others ?? new Boid[0];
        this._configData = configData;
        this.inputData = inputData;
        motion = new MotionMetrics();
    }

    public override bool IsDone => !motion.moving;

    public override void Begin()
    {
        motion.position = inputData.transform.position;
        motion.moving = true;
    }

    public override void Update(float deltaTime)
    {
        if (motion.moving)
        {
            if (CheckDistance(motion, inputData, _configData.arriveDistance))
            {
                motion.acceleration += Arrive(motion, inputData.target, deltaTime);
            }
            else
            {
                motion.acceleration += Seek(motion, inputData.target);
                motion.acceleration += Separate(_others);
            }

            SetPosAndForward(motion.GetFinalPosition(deltaTime, _configData), motion.direction);

            motion.acceleration = Vector3.zero;

            if (CheckDistance(motion, inputData, 0.001f))
            {
                CancelMove();
            }
        }
    }

    public override void End()
    {
    }

    public void SetOthers(Boid[] others)
    {
        this._others = others;
    }

    protected virtual void SetPosAndForward(Vector3 pos, Vector3 forward)
    {
        inputData.transform.position = pos;
        inputData.transform.forward = forward;
    }

    private bool CheckDistance(MotionMetrics motionMetrics, InputData inputData, float distance)
    {
        return (motionMetrics.position - inputData.target).sqrMagnitude < distance * distance;
    }

    public void CancelMove()
    {
        motion.velocity = Vector3.zero;
        motion.position = inputData.target;
        motion.moving = false;
    }

    public float Speed => _configData.maxSpeed;

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

    private Vector3 Separate(Boid[] others)
    {
        float desiredSeperation = _configData.spacing;
        var sum = new Vector3();
        int count = 0;
        foreach (var other in others)
        {
            float d = Vector3.Distance(motion.position, other.motion.position);
            if ((d > 0) && (d < desiredSeperation))
            {
                var diff = motion.position - other.motion.position;
                diff = diff.normalized;
                diff /= d;
                sum += diff;
                count++;
            }
        }

        if (count > 0)
        {
            sum /= count;
            sum = sum.normalized;
            sum *= _configData.maxSpeed;

            var desiredAcceleration = sum - motion.velocity;
            desiredAcceleration = SNM.Math.ClampMagnitude(desiredAcceleration, _configData.maxAcceleration);

            return desiredAcceleration;
        }

        return Vector3.zero;
    }

    protected struct MotionMetrics
    {
        public bool moving;

        public Vector3 position;
        public Vector3 velocity;
        public Vector3 acceleration;
        public Vector3 direction;

        public Vector3 GetFinalPosition(float deltaTime, Boid.ConfigData config)
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

    public struct InputData
    {
        public Vector3 target;
        public Transform transform;
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

public class JumpingBoid : Boid
{
    private Vector3 _boidingPos;
    private readonly PieceActor.Jump _jump;
    private bool _delay = false;
    private float _intervalTime = 0f;
    public override bool IsDone => base.IsDone && _jump.IsDone;
    private bool _noJumping;

    public JumpingBoid(ConfigData configData, InputData inputData, Boid[] others)
        : base(configData, inputData, others)
    {
        _jump = new PieceActor.Jump(inputData.transform, new PieceActor.Jump.InputData()
        {
            height = 0.3f,
            duration = 0.25f
        });
    }

    protected override void SetPosAndForward(Vector3 pos, Vector3 forward)
    {
        _boidingPos = pos;
        inputData.transform.forward = forward;

        var p = inputData.transform.position;
        inputData.transform.position = new Vector3(_boidingPos.x, p.y, _boidingPos.z);
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
        if (motion.moving)
        {
            if (!_noJumping && _jump.IsDone)
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
        if (Vector3.SqrMagnitude(inputData.target - motion.position) > jumpDistance * jumpDistance)
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