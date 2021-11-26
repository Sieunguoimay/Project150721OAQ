using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Boid : SNM.Activity
{
    protected MotionMetrics motion;

    private ConfigData configData;
    protected InputData inputData;
    private Boid[] others;

    public Boid(ConfigData configData, InputData inputData, Boid[] others)
    {
        this.others = others ?? new Boid[0];
        this.configData = configData;
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
            if (CheckDistance(motion, inputData, configData.arriveDistance))
            {
                motion.acceleration += Arrive(motion, inputData.target, deltaTime);
            }
            else
            {
                motion.acceleration += Seek(motion, inputData.target);
                motion.acceleration += Separate(others);
            }

            SetPosAndForward(motion.GetFinalPosition(deltaTime, configData), motion.direction);

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
        this.others = others;
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

    private Vector3 Arrive(MotionMetrics motion, Vector3 target, float deltaTime)
    {
        var diff = target - motion.position;
        var mag = diff.magnitude;
        var f = mag / configData.arriveDistance;
        var desiredVelocity = (diff / mag) * Mathf.Lerp(0f, configData.maxSpeed, 1 - (f - 1) * (f - 1));

        var desiredAcceleration = (desiredVelocity - motion.velocity) / deltaTime;

        return desiredAcceleration;
    }

    private Vector3 Seek(MotionMetrics motion, Vector3 target)
    {
        var diff = target - motion.position;
        var mag = diff.magnitude;
        var desiredVelocity = diff / mag;
        desiredVelocity *= configData.maxSpeed;

        var desiredAcceleration = desiredVelocity - motion.velocity;
        desiredAcceleration = SNM.Math.ClampMagnitude(desiredAcceleration, configData.maxAcceleration);

        return desiredAcceleration;
    }

    private Vector3 Separate(Boid[] others)
    {
        float desiredSeperation = configData.spacing;
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
            sum *= configData.maxSpeed;

            var desiredAcceleration = sum - motion.velocity;
            desiredAcceleration = SNM.Math.ClampMagnitude(desiredAcceleration, configData.maxAcceleration);

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
    private Vector3 boidingPos;
    private PieceActor.Jump jump;
    private bool delay = false;
    private float intervalTime = 0f;
    public override bool IsDone => base.IsDone && jump.IsDone;

    public JumpingBoid(ConfigData configData, InputData inputData, Boid[] others)
        : base(configData, inputData, others)
    {
        jump = new PieceActor.Jump(inputData.transform, new PieceActor.Jump.InputData()
        {
            height = 0.3f,
            duration = 0.25f
        });
    }

    protected override void SetPosAndForward(Vector3 pos, Vector3 forward)
    {
        boidingPos = pos;
        inputData.transform.forward = forward;

        var p = inputData.transform.position;
        inputData.transform.position = new Vector3(boidingPos.x, p.y, boidingPos.z);
    }

    public override void Begin()
    {
        base.Begin();
        jump.Begin();
    }

    public override void Update(float deltaTime)
    {
        jump.Update(deltaTime);
        if (motion.moving)
        {
            if (jump.IsDone)
            {
                delay = true;
            }
            else
            {
                base.Update(deltaTime);
            }
        }

        if (delay)
        {
            intervalTime += deltaTime;
            if (intervalTime >= 0.08f)
            {
                delay = false;
                intervalTime = 0f;
                jump.Begin();
            }
        }
    }
}