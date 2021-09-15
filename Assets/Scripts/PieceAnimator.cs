using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using DG.Tweening;
using SNM;
using UnityEngine;
using Animation = SNM.Animation;
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
        public float time;
        public float duration;
        private Transform transform;

        public JumpAnim(Transform transform, InputData inputData, IEasing easing = null)
        {
            this.transform = transform;
            this.inputData = inputData;
            if (easing != null)
            {
                SetEase(easing);
            }
        }

        public override void Begin()
        {
            float h = inputData.height;
            float t = inputData.duration;
            var pos = transform.position;
            float a = (-8f * h) / (t * t);

            time = 0;
            IsDone = false;
            duration = t;
            initialPosition = pos;
            initialAcceleration = Vector3.up * a;
            initialVelocity = Vector3.up * (-a * 0.5f * duration);
        }

        public override void Update(float deltaTime)
        {
            if (!IsDone)
            {
                time += deltaTime;

                float t = Mathf.Min(time / duration, 1f);

                // var xz = SNM.Math.MotionEquation(
                //     initialPosition, initialVelocity,
                //     initialAcceleration, time);

                var xz = transform.position;
                var y = SNM.Math.MotionEquation(
                    initialPosition, initialVelocity,
                    initialAcceleration, ease.GetEase(t) * duration);
                transform.position = new Vector3(xz.x, y.y, xz.z);

                if (time >= duration)
                {
                    IsDone = true;
                }
            }
        }

        public override void End()
        {
            inputData.callback?.Invoke(null, inputData.flag);
        }

        public class InputData
        {
            // public Vector3 target;
            public float height = 1f;
            public float duration = 0.4f;
            public int flag;
            public Action<PieceAnimator, int> callback;
        }
    }

    public class StraightMove : Animation
    {
        private Vector3 target;
        private Vector3 origin;
        private float duration;
        private Transform transform;

        private float time;

        public StraightMove(Transform transform, Vector3 target, float duration, IEasing ease = null)
        {
            this.transform = transform;
            this.target = target;
            this.duration = duration;
            if (ease != null)
            {
                SetEase(ease);
            }
        }

        public override void Begin()
        {
            base.Begin();
            time = 0;
            origin = transform.position;
            transform.rotation = Quaternion.LookRotation(SNM.Math.Projection(target - origin,
                Main.Instance.GameCommonConfig.UpVector));
        }

        public override void Update(float deltaTime)
        {
            if (!IsDone)
            {
                time += deltaTime;
                float t = Mathf.Min(time / duration, 1f);
                var pos = Vector3.Lerp(origin, target, ease.GetEase(t));
                pos.y = transform.position.y;
                transform.position = pos;
                if (time >= duration)
                {
                    IsDone = true;
                }
            }
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