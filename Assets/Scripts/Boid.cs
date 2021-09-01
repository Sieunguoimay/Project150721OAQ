using System;
using UnityEditor;
using UnityEngine;

public class Boid : SNM.IAnimation
{
    private Transform transform;

    private MotionMetrics motion;

    private ControlData controlData;

    private ConfigData configData;

    public Boid(Transform transform, InputData inputData)
    {
        this.transform = transform;
        motion = new MotionMetrics();
        configData = new ConfigData()
        {
            maxSpeed = 3f,
            maxAcceleration = 10f,
            arriveDistance = 1f
        };
        controlData = new ControlData
        {
            inputData = inputData,
            moving = true
        };
    }

    public bool IsDone => !controlData.moving;

    public void Start()
    {
        motion.position = transform.position;
    }

    public void Update(float deltaTime)
    {
        if (controlData.moving)
        {
            if (CheckDistance(motion, controlData.inputData, configData.arriveDistance))
            {
                motion.acceleration += Arrive(motion, controlData.inputData.target);
            }
            else
            {
                motion.acceleration += Seek(motion, controlData.inputData.target);
            }

            transform.position = motion.GetFinalPosition(deltaTime, configData);
            motion.acceleration = Vector3.zero;

            if (CheckDistance(motion, controlData.inputData, 0.001f))
            {
                CancelMove();
            }
        }
    }

    public void End()
    {
    }

    private bool CheckDistance(MotionMetrics motionMetrics, InputData inputData, float distance)
    {
        return (motionMetrics.position - inputData.target).sqrMagnitude < distance * distance;
    }

    public void CancelMove()
    {
        motion.velocity = Vector3.zero;
        motion.position = controlData.inputData.target;
        controlData.moving = false;
        controlData.inputData.onDone?.Invoke();
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

    private struct MotionMetrics
    {
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 acceleration;

        public Vector3 GetFinalPosition(float deltaTime, Boid.ConfigData config)
        {
            velocity += acceleration * deltaTime;
            velocity = SNM.Math.ClampMagnitude(velocity, config.maxSpeed);
            position += velocity * deltaTime;
            return position;
        }
    }

    public struct InputData
    {
        public Vector3 target;
        public Action onDone;
    }

    private struct ControlData
    {
        public InputData inputData;
        public bool moving;
    }

    [Serializable]
    public struct ConfigData
    {
        public float maxSpeed;
        public float maxAcceleration;
        public float arriveDistance;
    }
}