using System;
using UnityEditor;
using UnityEngine;

public class Boid : SNM.Animation
{
    private MotionMetrics motion;

    private ConfigData configData;
    private InputData inputData;
    private Boid[] others;

    public Boid(ConfigData configData, InputData inputData, Boid[] others)
    {
        this.others = others;
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
                motion.acceleration += Arrive(motion, inputData.target);
            }
            else
            {
                motion.acceleration += Seek(motion, inputData.target);
                motion.acceleration += Separate(others);
            }

            inputData.transform.position = motion.GetFinalPosition(deltaTime, configData);
            inputData.transform.forward = motion.direction;
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

    private Vector3 Arrive(MotionMetrics motion, Vector3 target)
    {
        var diff = target - motion.position;
        var mag = diff.magnitude;
        var f = mag / configData.arriveDistance;
        var desiredVelocity = (diff / mag) * Mathf.Lerp(0f, configData.maxSpeed, 1 - (f - 1) * (f - 1));

        var desiredAcceleration = (desiredVelocity - motion.velocity) / Time.deltaTime;

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

    private struct MotionMetrics
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
                direction = velocity;
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