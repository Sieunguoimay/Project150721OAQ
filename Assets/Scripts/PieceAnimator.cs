using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using DG.Tweening;
using SNM;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class PieceAnimator : SNM.Animator
{
    private ConfigData config = new ConfigData() {gravity = -10f, angularSpeed = 270f};

    private Transform transform;
    public event Action<bool> OnJump = delegate(bool last) { };
    public bool IsJumping => currentAnim != null;

    public ConfigData Config => config;

    public PieceAnimator(Transform transform)
    {
        this.transform = transform;
    }

    protected override void OnNewAnim(IAnimation anim)
    {
        if (anim is JumpAnim ja)
        {
            OnJump?.Invoke(ja.jumpTarget.flag > 0);
        }
    }

    public class JumpAnim : SNM.IAnimation
    {
        public JumpTarget jumpTarget;
        public Vector3 initialPosition;
        public Vector3 initialVelocity;
        public Vector3 initialAcceleration;
        public bool done;
        public float time;
        public float duration;
        private Transform transform;
        public bool IsDone => done;

        public JumpAnim(Transform transform, JumpTarget jumpTarget)
        {
            this.transform = transform;
            this.jumpTarget = jumpTarget;
        }

        public void Start()
        {
            float h = jumpTarget.height;
            float t = 0.3f;
            var pos = transform.position;
            var dir = jumpTarget.target - pos;
            float a = (-8f * h) / (t * t); //-v * v / (2f * h);

            time = 0;
            done = false;
            duration = t;
            initialPosition = pos;
            initialAcceleration = Vector3.up * a;
            initialVelocity =
                dir / duration + Vector3.up * (-a * 0.5f * duration);

            transform.rotation =
                UnityEngine.Quaternion.LookRotation(SNM.Math.Projection(jumpTarget.target - pos,
                    Main.Instance.GameCommonConfig.UpVector));
        }

        public void Update(float deltaTime)
        {
            if (!done)
            {
                time += deltaTime;


                if (time >= duration)
                {
                    done = true;
                    time = duration;
                    transform.position = SNM.Math.MotionEquation(initialPosition, initialVelocity,
                        initialAcceleration, time);
                }
                else
                {
                    transform.position = SNM.Math.MotionEquation(initialPosition, initialVelocity,
                        initialAcceleration, time);
                }
            }
        }

        public void End()
        {
            jumpTarget.onDone?.Invoke(null, jumpTarget.flag);
        }
    }

    public class Delay : SNM.IAnimation
    {
        private float duration;
        private float time = 0;

        public bool IsDone { get; private set; } = false;

        public Delay(float duration)
        {
            this.duration = duration;
        }

        public void Start()
        {
        }

        public void Update(float deltaTime)
        {
            time += deltaTime;
            if (time >= duration)
            {
                IsDone = true;
            }
        }

        public void End()
        {
        }
    }

    [Serializable]
    public struct ConfigData
    {
        public float gravity;
        public float angularSpeed;
    }

    public class JumpTarget
    {
        public Vector3 target;
        public float height = 1f;
        public int flag;
        public Action<PieceAnimator, int> onDone;
    }
}