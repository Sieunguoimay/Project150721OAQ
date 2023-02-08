using System;
using System.Linq;
using Common;
using Common.Activity;
using DG.Tweening;
using SNM.Easings;
using Timeline;
using UnityEngine;
using UnityEngine.Timeline;
using Vector3 = UnityEngine.Vector3;

namespace Gameplay.Piece.Activities
{
    #region Uncommon

    public sealed class LinearEasing : IEasing
    {
        public float GetEase(float x)
        {
            return x;
        }
    }

    public abstract class AEasingActivity : Activity
    {
        protected readonly IEasing Ease;

        protected AEasingActivity(IEasing ease)
        {
            Ease = ease;
        }
    }

    public class StraightMove : AEasingActivity
    {
        protected Vector3 Target { get; }
        private Vector3 _origin;
        protected Transform Transform { get; }

        private float _time;
        protected float Duration { get; set; }

        protected StraightMove(Transform transform, Vector3 target, float duration, IEasing ease) : base(ease)
        {
            Transform = transform;
            Target = target;
            Duration = duration;
        }


        public override void Begin()
        {
            base.Begin();
            _time = 0;
            _origin = Transform.position;
            Transform.rotation = Quaternion.LookRotation(SNM.Math.Projection(Target - _origin, Vector3.up));
        }

        public override void Update(float deltaTime)
        {
            if (Inactive) return;

            _time += deltaTime;
            var t = Mathf.Min(_time / Duration, 1f);

            UpdatePosition(t);

            if (_time >= Duration)
            {
                End();
            }
        }

        protected virtual void UpdatePosition(float t)
        {
            var pos = Vector3.Lerp(_origin, Target, Ease.GetEase(t));
            pos.y = Transform.position.y;
            Transform.position = pos;
        }
    }

    public class StraightMoveBySpeed : StraightMove
    {
        private readonly float _speed;

        protected StraightMoveBySpeed(Transform transform, Vector3 target, float speed, IEasing ease) : base(transform,
            target, 1f, ease)
        {
            _speed = speed;
        }

        public override void Begin()
        {
            Duration = Vector3.Distance(Target, Transform.position) / _speed;
            base.Begin();
            Debug.Log(Duration);
        }
    }

    public class JumpForward : StraightMove
    {
        private Vector3 _initialPosition;
        private Vector3 _initialVelocity;
        private Vector3 _initialAcceleration;
        private readonly float _height;
        private readonly IEasing _jumpEase;

        public JumpForward(Transform transform, Vector3 target, float duration, IEasing moveEase, float height,
            IEasing jumpEase) : base(transform, target, duration, moveEase)
        {
            _height = height;
            _jumpEase = jumpEase;
        }

        public override void Begin()
        {
            base.Begin();
            var pos = Transform.position;
            var distance = Vector3.Distance(Target, pos);
            var h = Mathf.Clamp(distance, _height / 2f, _height);
            var t = Duration;
            var a = (-8f * h) / (t * t);

            _initialPosition = pos;
            _initialAcceleration = Vector3.up * a;
            _initialVelocity = Vector3.up * (-a * 0.5f * Duration);
        }

        protected override void UpdatePosition(float t)
        {
            var xz = Transform.position;
            var y = SNM.Math.MotionEquation(
                _initialPosition, _initialVelocity,
                _initialAcceleration, _jumpEase.GetEase(t) * Duration);
            Transform.position = new Vector3(xz.x, y.y, xz.z);
            base.UpdatePosition(t);
        }
    }

    #endregion Uncommon

    public class ActivityJump : AEasingActivity
    {
        private readonly InputData _inputData;
        private Vector3 _initialPosition;
        private Vector3 _initialVelocity;
        private Vector3 _initialAcceleration;
        private float _time;
        private float _duration;
        private readonly Transform _transform;

        public ActivityJump(Transform transform, InputData inputData, IEasing easing) : base(easing)
        {
            _transform = transform;
            _inputData = inputData;
        }

        public override void Begin()
        {
            var h = _inputData.Height;
            var t = _inputData.Duration;
            var pos = _transform.position;
            var a = (-8f * h) / (t * t);

            _time = 0;
            Inactive = false;
            _duration = t;
            _initialPosition = pos;
            _initialAcceleration = Vector3.up * a;
            _initialVelocity = Vector3.up * (-a * 0.5f * _duration);
        }

        public override void Update(float deltaTime)
        {
            if (Inactive) return;

            _time += deltaTime;

            var t = Mathf.Min(_time / _duration, 1f);

            var xz = _transform.position;
            var y = SNM.Math.MotionEquation(
                _initialPosition, _initialVelocity,
                _initialAcceleration, Ease.GetEase(t) * _duration);
            _transform.position = new Vector3(xz.x, y.y, xz.z);

            if (_time >= _duration)
            {
                Inactive = true;
            }
        }

        public float GetJumpDistance(float movingSpeed) => _duration * movingSpeed;

        public class InputData
        {
            public float Height = 1f;
            public float Duration = 0.4f;
        }
    }

    public class ActivityTurnAway : Activity
    {
        private readonly Transform _transform;

        public ActivityTurnAway(Transform transform)
        {
            _transform = transform;
        }

        public override void Begin()
        {
            base.Begin();

            var lr = _transform.localEulerAngles;
            lr.y += UnityEngine.Random.Range(-60f, 60f);
            _transform.DOLocalRotate(lr, 1f).SetLink(_transform.gameObject)
                .OnComplete(() => { Inactive = true; });
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

    public class ActivityBounceAnim : Activity
    {
        private readonly Transform _transform;
        private readonly float _duration;
        private float _time;
        private readonly float _offset;
        private readonly bool _fullPhase;

        public ActivityBounceAnim(Transform transform, float duration, bool fullPhase = false)
        {
            _transform = transform;
            _duration = duration;
            _offset = 0.3f;
            _fullPhase = fullPhase;
        }

        public override void Update(float deltaTime)
        {
            if (Inactive) return;

            _time += deltaTime;
            var t = Mathf.Min(_time / _duration, 1f);

            var scale = _transform.localScale;
            if (_fullPhase)
            {
                var s = Mathf.Sin(Mathf.Lerp(0, Mathf.PI * 2f, t));
                scale.y = 1 + (-s) * _offset;
                scale.x = 1 + (s) * _offset * 0.35f;
                scale.z = 1 + (s) * _offset * 0.35f;
            }
            else
            {
                var c = Mathf.Cos(Mathf.Lerp(0, Mathf.PI * 2f, t));
                scale.y = 1 + (c) * _offset * 0.5f;
                scale.x = 1 + (-c) * _offset * 0.25f;
                scale.z = 1 + (-c) * _offset * 0.25f;
            }

            _transform.localScale = scale;

            if (_time >= _duration)
            {
                Inactive = true;
            }
        }
    }

    public class ActivityRotateToTarget : Activity
    {
        private readonly Vector3 _targetPos;
        private readonly Transform _transform;
        private readonly float _duration;

        public ActivityRotateToTarget(Transform transform, Vector3 pos, float duration)
        {
            _transform = transform;
            _targetPos = pos;
            _duration = duration;
        }

        public override void Begin()
        {
            base.Begin();

            var euler = Quaternion.LookRotation(_targetPos - _transform.position).eulerAngles;
            var targetEuler = _transform.eulerAngles;
            targetEuler.y = euler.y;
            _transform.transform.DORotate(targetEuler, _duration).SetLink(_transform.gameObject).OnComplete(End);
        }
    }

    public class ActivityCallback : Activity
    {
        private readonly Action _callback;

        public ActivityCallback(Action callback)
        {
            _callback = callback;
        }

        public override void Begin()
        {
            base.Begin();
            _callback?.Invoke();
            End();
        }
    }

    public class ActivityJumpTimeline : ActivityDelay
    {
        private readonly Citizen _p;
        private readonly Func<Vector3> _target;

        private TransformControlTrack _jumping;
        private TransformControlTrack _facing;

        public ActivityJumpTimeline(Citizen p, Func<Vector3> target) : base((float) p.JumpTimeline.duration)
        {
            _p = p;
            _target = target;
        }

        public override void Begin()
        {
            base.Begin();
            
            var target = _target.Invoke();
            
            var tracks =
                _p.JumpTimeline.playableAsset.outputs.Where(tr => tr.sourceObject is TransformControlTrack)
                    .Select(tr => tr.sourceObject as TransformControlTrack).ToArray();

            _jumping = tracks.FirstOrDefault(t => t.label.Equals("jumping"));
            _facing = tracks.FirstOrDefault(t => t.label.Equals("facing"));


            _p.JumpTimeline.Stop();
            var euler = Quaternion
                .LookRotation(_p.transform.InverseTransformDirection(target - _p.transform.position))
                .eulerAngles;

            SetTrack(_jumping, target.x, target.z, 0);
            SetTrack(_facing, 0, 0,
                ClampEuler(euler.y, ((Transform) _p.JumpTimeline.GetGenericBinding(_facing)).localEulerAngles.y));

            var transform = _p.transform;
            var pos = transform.position;
            pos.y = 0f;
            transform.position = pos;

            _p.JumpTimeline.Play();
        }

        public override void End()
        {
            base.End();
            var clips = _jumping.GetClips().Concat(_facing.GetClips());
            foreach (var c in clips)
            {
                var clip = ((TransformControlClip) c.asset);
                clip.Template.position = Vector3.zero;
                clip.Template.eulerAngles = Vector3.zero;
            }
        }

        private static float ClampEuler(float newEuler, float oldEuler)
        {
            var offset = newEuler - oldEuler;
            return offset switch
            {
                > 180f => newEuler - 360f,
                < -180f => newEuler + 360f,
                _ => newEuler
            };
        }

        private static void SetTrack(TrackAsset tr, float x, float z, float eulerY)
        {
            if (tr == null) return;
            var clips = tr.GetClips();
            foreach (var c in clips)
            {
                ((TransformControlClip) c.asset).Template.position.x = x;
                ((TransformControlClip) c.asset).Template.position.z = z;
                ((TransformControlClip) c.asset).Template.eulerAngles.y = eulerY;
            }
        }
    }

    public class ActivityAnimation : Activity
    {
        private readonly Animator _animator;
        private readonly int _animHash;

        public ActivityAnimation(Animator animator, int animHash)
        {
            _animator = animator;
            _animHash = animHash;
        }

        public override void Begin()
        {
            base.Begin();
            _animator.Play(_animHash, -1, 0f);
        }

        public override void Update(float deltaTime)
        {
            var info = _animator.GetCurrentAnimatorStateInfo(0);
            if (info.shortNameHash != _animHash) return;

            if (info.loop)
                Debug.LogError("Loop... :(");

            if (info.normalizedTime >= 1f)
            {
                End();
            }
        }
    }
}