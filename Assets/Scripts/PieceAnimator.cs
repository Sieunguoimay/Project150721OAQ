using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using DG.Tweening;
using SNM;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class PieceAnimator : SNM.Animator
{
    private ConfigData config = new ConfigData() {gravity = -10f, angularSpeed = 270f};

    public event Action<bool> OnJump = delegate(bool last) { };
    public bool IsJumping => currentAnim != null;

    public ConfigData Config => config;

    protected override void OnNewAnim(SNM.Animation anim)
    {
        if (anim is JumpAnim ja)
        {
            OnJump?.Invoke(ja.inputData.flag > 0);
        }
    }

    public class JumpAnim : SNM.Animation
    {
        public InputData inputData;
        public Vector3 initialPosition;
        public Vector3 initialVelocity;
        public Vector3 initialAcceleration;
        public bool done;
        public float time;
        public float duration;
        private Transform transform;
        public override bool IsDone => done;

        public JumpAnim(Transform transform, InputData inputData, IEasing linearEasing = null)
        {
            this.transform = transform;
            this.inputData = inputData;
            if (linearEasing != null)
            {
                SetEase(linearEasing);
            }
        }

        public override void Begin()
        {
            float h = inputData.height;
            float t = inputData.duration;
            var pos = transform.position;
            var dir = inputData.target - pos;
            float a = (-8f * h) / (t * t);

            time = 0;
            done = false;
            duration = t;
            initialPosition = pos;
            initialAcceleration = Vector3.up * a;
            initialVelocity =
                dir / duration + Vector3.up * (-a * 0.5f * duration);

            transform.rotation =
                UnityEngine.Quaternion.LookRotation(SNM.Math.Projection(inputData.target - pos,
                    Main.Instance.GameCommonConfig.UpVector));
            // Main.Instance.Delay(duration * 0.25f, () =>
            // {
            //     transform.DOLocalRotate(transform.eulerAngles + new Vector3(360f, 0, 0), duration * 0.5f,
            //         RotateMode.FastBeyond360);
            // });
        }

        public override void Update(float deltaTime)
        {
            if (!done)
            {
                time += deltaTime;

                if (time >= duration)
                {
                    done = true;
                    time = duration;
                }

                var xz = SNM.Math.MotionEquation(
                    initialPosition, initialVelocity,
                    initialAcceleration, time);

                var y = SNM.Math.MotionEquation(
                    initialPosition, initialVelocity,
                    initialAcceleration, ease.GetEase(time / duration) * duration);
                transform.position = new Vector3(xz.x, y.y, xz.z);
            }
        }

        public override void End()
        {
            inputData.callback?.Invoke(null, inputData.flag);
        }

        public class InputData
        {
            public Vector3 target;
            public float height = 1f;
            public float duration = 0.4f;
            public int flag;
            public Action<PieceAnimator, int> callback;
        }
    }

    public class Delay : SNM.Animation
    {
        private float duration;
        private float time = 0;

        public Delay(float duration)
        {
            this.duration = duration;
        }

        public override void Update(float deltaTime)
        {
            time += deltaTime;
            if (time >= duration)
            {
                IsDone = true;
            }
        }
    }

    public class TurnAway : SNM.Animation
    {
        private Transform transform;

        public TurnAway(Transform transform)
        {
            this.transform = transform;
        }

        public override void Begin()
        {
            base.Begin();

            var lr = transform.localEulerAngles;
            lr.y += UnityEngine.Random.Range(-60f, 60f);
            transform.DOLocalRotate(lr, 1f).SetId(this)
                .OnComplete(() => { IsDone = true; });
        }

        public override void Update(float deltaTime)
        {
        }

        public override void End()
        {
            base.End();
            DOTween.Kill(this);
        }
    }

    [Serializable]
    public struct ConfigData
    {
        public float gravity;
        public float angularSpeed;
    }
}