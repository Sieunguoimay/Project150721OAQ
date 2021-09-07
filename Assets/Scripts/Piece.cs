using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Piece : Prefab
{
    public PieceAnimator PieceAnimator { get; private set; }

    private Animator animator;
    private Animator Animator => animator ? animator : (animator = GetComponentInChildren<Animator>());

    private ConfigData configData;
    public ConfigData ConfigDataProp => configData;

    private bool isRandomlyRotating = false;

    public void Setup(ConfigData configData)
    {
        this.configData = configData;
        this.Delay(UnityEngine.Random.Range(0.1f, 2f), () => Animator?.Play("idle"));
        FaceCamera(true, new Vector3(0, UnityEngine.Random.Range(-45f, 45f), 0));

        PieceAnimator = new PieceAnimator();
        PieceAnimator.OnJump += OnJump;
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
                    if (!(PieceAnimator?.IsJumping ?? false))
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
        PieceAnimator?.Update(Time.deltaTime);
    }

    public void FaceCamera(bool immediate, Vector3 offset = new Vector3())
    {
        if (Main.Instance.References.Camera != null)
        {
            var dir = Main.Instance.References.Camera.transform.position - transform.position;
            var up = transform.up;
            dir = SNM.Math.Projection(dir, up);
            if (immediate)
            {
                transform.rotation = Quaternion.LookRotation(dir, up);
            }
            else
            {
                var target = Quaternion.LookRotation(dir, up).eulerAngles + offset;
                var duration = (target - transform.eulerAngles).magnitude / PieceAnimator.Config.angularSpeed;
                transform.DORotate(target, duration);
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
}