using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using DG.Tweening;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class Mover
{
    private Queue<JumpTarget> queue = new Queue<JumpTarget>();

    private JumpData jumpData = new JumpData() {done = true};
    private ConfigData config = new ConfigData() {gravity = -10f, angularSpeed = 270f};

    private Transform transform;
    public event Action<bool> OnJump = delegate(bool last) { };
    public event Action<bool> OnJumpEnd = delegate(bool last) { };
    public bool IsJumpingInQueue { get; private set; } = false;
    public bool IsJumping => !jumpData.done;

    public ConfigData Config => config;

    public Mover(Transform transform)
    {
        this.transform = transform;
    }

    public void JumpTo(Vector3 position, Action onComplete)
    {
        // transform.DOJump(position, 1f, 1, 0.3f).OnComplete(() => onComplete?.Invoke());
        float h = 1f;
        float t = 0.3f;
        var pos = transform.position;
        var dir = position - pos;
        float a = (-8f * h) / (t * t); //-v * v / (2f * h);

        jumpData.time = 0;
        jumpData.done = false;
        jumpData.duration = t;
        jumpData.onComplete = onComplete;
        jumpData.initialPosition = pos;
        jumpData.initialAcceleration = Vector3.up * a;
        jumpData.initialVelocity =
            dir / jumpData.duration + Vector3.up * (-a * 0.5f * jumpData.duration);

        transform.rotation =
            UnityEngine.Quaternion.LookRotation(SNM.Math.Projection(position - pos,
                Main.Instance.GameCommonConfig.UpVector));
    }

    public void Update(float deltaTime)
    {
        if (!jumpData.done)
        {
            jumpData.time += deltaTime;


            if (jumpData.time >= jumpData.duration)
            {
                jumpData.done = true;
                jumpData.time = jumpData.duration;
                transform.position = SNM.Math.MotionEquation(jumpData.initialPosition, jumpData.initialVelocity,
                    jumpData.initialAcceleration, jumpData.time);
                jumpData.onComplete?.Invoke();
            }
            else
            {
                transform.position = SNM.Math.MotionEquation(jumpData.initialPosition, jumpData.initialVelocity,
                    jumpData.initialAcceleration, jumpData.time);
            }
        }
    }

    public void EnqueueTarget(JumpTarget p) => queue.Enqueue(p);

    public void JumpInQueue()
    {
        if (queue.Count > 0)
        {
            IsJumpingInQueue = true;
            var target = queue.Dequeue();
            JumpTo(target.target, () =>
            {
                JumpInQueue();
                OnJumpEnd?.Invoke(target.flag == 1);
            });
            OnJump?.Invoke(target.flag == 1);
        }
        else
        {
            IsJumpingInQueue = false;
        }
    }

    public struct JumpData
    {
        public Action onComplete;
        public Vector3 initialPosition;
        public Vector3 initialVelocity;
        public Vector3 initialAcceleration;
        public bool done;
        public float time;
        public float duration;
    }

    [Serializable]
    public struct ConfigData
    {
        public float gravity;
        public float angularSpeed;
    }

    public struct JumpTarget
    {
        public Vector3 target;
        public int flag;
    }
}