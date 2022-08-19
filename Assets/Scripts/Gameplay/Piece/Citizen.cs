﻿using Common;
using Common.Animation;
using Common.ResolveSystem;
using DG.Tweening;
using UnityEngine;

namespace Gameplay
{
    [SelectionBase]
    public class Citizen : Piece.Piece
    {
        [SerializeField] private AnimatorListener animatorListener;

        private Transform _cameraTransform;
        private Animator _animator;
        public override Animator Animator => _animator ??= animatorListener.GetComponent<Animator>();

        public override void Setup()
        {
            base.Setup();
            // FaceCamera(_cameraTransform.position,true, new Vector3(0, UnityEngine.Random.Range(-45f, 45f), 0));
            // _cameraTransform = Resolver.Instance.Resolve<CameraManager>().transform;
        }

        public void FaceCamera(Vector3 pos, bool immediate, Vector3 offset = new())
        {
            var t = transform;
            var dir = pos - t.position;
            var up = t.up;
            dir = SNM.Math.Projection(dir, up);
            if (immediate)
            {
                transform.rotation = Quaternion.LookRotation(dir, up);
            }
            else
            {
                var target = Quaternion.LookRotation(dir, up).eulerAngles + offset;
                var duration = (target - transform.eulerAngles).magnitude / PieceActivityQueue.Config.angularSpeed;
                transform.DORotate(target, duration);
            }
        }
    }
}