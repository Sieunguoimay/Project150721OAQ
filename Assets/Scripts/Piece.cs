using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = System.Random;

public class Piece : Prefab
{
    private Mover mover;
    public Mover Mover => mover ?? (mover = new Mover(transform));

    private Animator animator;
    private Animator Animator => animator ? animator : (animator = GetComponentInChildren<Animator>());
    private bool isRandomlyRotating = false;

    protected override void Start()
    {
        base.Start();
        this.Delay(UnityEngine.Random.Range(0.1f, 2f), () => { Animator?.Play("idle"); });
        Mover.OnJump += OnJump;
        FaceCamera(true, new Vector3(0, UnityEngine.Random.Range(-45f, 45f), 0));
    }

    private void OnJump(bool last)
    {
        if (isRandomlyRotating)
        {
            DOTween.Kill(nameof(isRandomlyRotating));
        }

        if (last)
        {
            this.Delay(0.2f, () =>
            {
                Animator?.CrossFade("land", 0.1f);
                this.Delay(UnityEngine.Random.Range(0.5f, 1f), () =>
                {
                    if (!(Mover?.IsJumping ?? false))
                    {
                        isRandomlyRotating = true;

                        var lr = transform.localEulerAngles;
                        lr.y += UnityEngine.Random.Range(-60f, 60f);
                        transform.DOLocalRotate(lr, 1f)
                            .SetId(nameof(isRandomlyRotating))
                            .OnComplete(() => { isRandomlyRotating = false; });
                    }
                });
            });
        }
    }

    private void Update()
    {
        Mover?.Update(Time.deltaTime);
    }

    public void FaceCamera(bool immediate, Vector3 offset = new Vector3())
    {
        var t = transform;
        if (!(Camera.main is null))
        {
            var dir = Camera.main.transform.position - t.position;
            var up = t.up;
            dir = SNM.Math.Projection(dir, up);
            if (immediate)
            {
                transform.rotation = Quaternion.LookRotation(dir, up);
            }
            else
            {
                var target = Quaternion.LookRotation(dir, up).eulerAngles + offset;
                var duration = (target - transform.eulerAngles).magnitude / Mover.Config.angularSpeed;
                transform.DORotate(target, duration);
            }
        }
    }

#if UNITY_EDITOR
    private Vector3 initialPosition;
    [ContextMenu("Test Jump")]
    private void TestJump()
    {
        initialPosition = transform.position;
        this.ExecuteInNextFrame(() =>
        {
            Mover.JumpTo(initialPosition + Vector3.right * 2f,
                () => { this.Delay(1f, () => { transform.position = initialPosition; }); });
        });
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
}