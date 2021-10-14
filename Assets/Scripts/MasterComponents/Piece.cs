using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using SNM;
using UnityEngine;
using Animator = UnityEngine.Animator;

public class Piece : MasterComponent, Drone.IPickedUpObject
{
    [SerializeField] private Transform pickupPoint;

    public PieceActor PieceActor { get; private set; }

    private Animator animator;
    private Animator Animator => animator ? animator : (animator = GetComponentInChildren<Animator>());

    private ConfigData configData;
    public ConfigData ConfigDataProp => configData;

    private bool isRandomlyRotating = false;
    private Tag[] taggedGameObjects;
    private Transform footTransform;

    public void Setup(ConfigData configData)
    {
        this.configData = configData;
        this.Delay(UnityEngine.Random.Range(0.1f, 2f), () => Animator?.Play("idle"));
        FaceCamera(true, new Vector3(0, UnityEngine.Random.Range(-45f, 45f), 0));

        PieceActor = new PieceActor();
        taggedGameObjects = GetComponentsInChildren<Tag>();
        footTransform = taggedGameObjects.FirstOrDefault(t => t.ID.Equals("foot"))?.transform;
    }

    private void Update()
    {
        PieceActor?.Update(Time.deltaTime);
    }

    public void FaceCamera(bool immediate, Vector3 offset = new Vector3())
    {
        if (Main.Instance.References.camera != null)
        {
            var dir = Main.Instance.References.camera.transform.position - transform.position;
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
            }, BezierEasing.Blueprint1));
        parallelAnimation.Add(new CommonActivities.StraightMove(transform, pos, 0.4f));

        var sA = new SequentialActivity();
        sA.Add(new CommonActivities.Delay(0.1f));
        sA.Add(parallelAnimation);

        var parallelAnimation2 = new ParallelActivity();
        parallelAnimation2.Add(new BounceAnim(footTransform, 0.15f));
        parallelAnimation2.Add(sA);

        PieceActor.Add(parallelAnimation2);
    }

    public void Land()
    {
        PieceActor.Add(new BounceAnim(footTransform, 0.15f));
        PieceActor.Add(new PieceActor.TurnAway(transform));
    }

    public class BounceAnim : Activity
    {
        private Transform transform;
        private float duration;
        private float time;
        private float offset;
        private bool fullPhase;

        public BounceAnim(Transform transform, float duration, bool fullPhase = false)
        {
            this.transform = transform;
            this.duration = duration;
            this.offset = 0.3f;
            this.fullPhase = fullPhase;
        }

        public override void Update(float deltaTime)
        {
            if (!IsDone)
            {
                time += deltaTime;
                float t = Mathf.Min(time / duration, 1f);

                var scale = transform.localScale;
                if (fullPhase)
                {
                    var s = Mathf.Sin(Mathf.Lerp(0, Mathf.PI * 2f, t));
                    scale.y = 1 + (-s) * offset;
                    scale.x = 1 + (s) * offset * 0.35f;
                    scale.z = 1 + (s) * offset * 0.35f;
                }
                else
                {
                    var c = Mathf.Cos(Mathf.Lerp(0, Mathf.PI * 2f, t));
                    scale.y = 1 + (c) * offset * 0.5f;
                    scale.x = 1 + (-c) * offset * 0.25f;
                    scale.z = 1 + (-c) * offset * 0.25f;
                }

                transform.localScale = scale;

                if (time >= duration)
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
        private Piece piece;

        private bool isDeselected;

        public PieceToTileSelectorAdaptor(Piece piece)
        {
            this.piece = piece;
            isDeselected = false;
        }

        public void OnTileSelected()
        {
            piece.FaceCamera(false, new Vector3(0, UnityEngine.Random.Range(-25f, 25f), 0));
            piece.Delay(UnityEngine.Random.Range(0, 0.5f), () =>
                {
                    if (!isDeselected)
                    {
                        piece.Animator?.CrossFade("jump", 0.1f);
                    }
                }
            );
        }

        public void OnTileDeselected()
        {
            piece.Animator?.CrossFade("idle", 0.1f);
            isDeselected = true;
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

    public void OnDrop(Transform oldParent)
    {
        Debug.Log("On Dropped");
        transform.SetParent(oldParent);
    }

    public Transform Transform => pickupPoint;
}