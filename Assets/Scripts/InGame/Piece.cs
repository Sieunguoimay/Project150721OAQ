using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using SNM;
using UnityEngine;
using Animator = UnityEngine.Animator;

[RequireComponent(typeof(Animator))]
public class Piece : MasterComponent, Drone.IPickedUpObject
{
    [SerializeField] private Transform pickupPoint;

    public PieceActor PieceActor { get; private set; }

    // private Animator _animator;
    // private Animator Animator => _animator ? _animator : (_animator = GetComponentInChildren<Animator>());

    private ConfigData _configData;
    public ConfigData ConfigDataProp => _configData;

    private Tag[] _taggedGameObjects;
    private Transform _footTransform;
    private Camera _camera;

    private void Start()
    {
        _camera = Camera.main;
    }

    public void Setup(ConfigData configData)
    {
        _configData = configData;
        // this.Delay(UnityEngine.Random.Range(0.1f, 2f), () => Animator.Play("idle"));
        FaceCamera(true, new Vector3(0, UnityEngine.Random.Range(-45f, 45f), 0));

        PieceActor = new PieceActor();
        _taggedGameObjects = GetComponentsInChildren<Tag>();
        _footTransform = _taggedGameObjects.FirstOrDefault(t => t.ID.Equals("foot"))?.transform;
    }

    private void Update()
    {
        PieceActor?.Update(Time.deltaTime);
    }

    public void FaceCamera(bool immediate, Vector3 offset = new Vector3())
    {
        if (_camera != null)
        {
            var dir = _camera.transform.position - transform.position;
            var up = transform.up;
            dir = SNM.Math.Projection(dir, up);
            if (immediate)
            {
                transform.rotation = Quaternion.LookRotation(dir, up);
            }
            else
            {
                var target = Quaternion.LookRotation(dir, up).eulerAngles + offset;
                var duration = (target - transform.eulerAngles).magnitude / PieceActor.Config.angularSpeed;
                transform.DORotate(target, duration);
            }
        }
    }

    public void JumpTo(Vector3 pos, int flag, Action<PieceActor, int> callback)
    {
        var parallelAnimation = new ParallelActivity();
        parallelAnimation.Add(new PieceActor.Jump(transform,
            new PieceActor.Jump.InputData
            {
                flag = flag,
                callback = callback,
                duration = 0.4f
            }, 
            BezierEasing.Blueprint1));
        parallelAnimation.Add(new CommonActivities.StraightMove(transform, pos, 0.4f));

        var sA = new SequentialActivity();
        sA.Add(new CommonActivities.Delay(0.1f));
        sA.Add(parallelAnimation);

        var parallelAnimation2 = new ParallelActivity();
        parallelAnimation2.Add(new BounceAnim(_footTransform, 0.15f));
        parallelAnimation2.Add(sA);

        PieceActor.Add(parallelAnimation2);
    }

    public void Land()
    {
        PieceActor.Add(new BounceAnim(_footTransform, 0.15f));
        PieceActor.Add(new PieceActor.TurnAway(transform));
    }

    private class BounceAnim : Activity
    {
        private readonly Transform _transform;
        private readonly float _duration;
        private float _time;
        private readonly float _offset;
        private readonly bool _fullPhase;

        public BounceAnim(Transform transform, float duration, bool fullPhase = false)
        {
            _transform = transform;
            _duration = duration;
            _offset = 0.3f;
            _fullPhase = fullPhase;
        }

        public override void Update(float deltaTime)
        {
            if (!IsDone)
            {
                _time += deltaTime;
                float t = Mathf.Min(_time / _duration, 1f);

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
                    IsDone = true;
                }
            }
        }
    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        if (ConfigDataProp != null)
            Gizmos.DrawWireCube(transform.position, ConfigDataProp.size);
    }
#endif
    public class PieceToTileSelectorAdaptor : TileSelector.ISelectionAdaptor
    {
        private readonly Piece _piece;

        private bool _isDeselected;

        public PieceToTileSelectorAdaptor(Piece piece)
        {
            _piece = piece; 
            _isDeselected = false;
        }

        public void OnTileSelected()
        {
            _piece.FaceCamera(false, new Vector3(0, UnityEngine.Random.Range(-25f, 25f), 0));
            _piece.Delay(UnityEngine.Random.Range(0, 0.5f), () =>
                {
                    if (!_isDeselected)
                    {
                        // _piece.Animator.CrossFade("jump", 0.1f);
                    }
                }
            );
        }

        public void OnTileDeselected()
        {
            // _piece.Animator.CrossFade("idle", 0.1f);
            _isDeselected = true;
        }
    }

    [Serializable]
    public class ConfigData
    {
        public ConfigData(ConfigData prototype)
        {
            point = prototype.point;
            size = prototype.size;
        }

        public int point;
        public Vector3 size;
    }

    public void OnPick(Transform attachTarget)
    {
        Debug.Log("On Picked");
        transform.SetParent(attachTarget);
    }

    public void OnDrop(Transform oldParent, Placement targetPlacement)
    {
        Debug.Log("On Dropped");
        transform.SetParent(oldParent);
        transform.position = targetPlacement.Position;
    }

    public Transform Transform => pickupPoint;
}