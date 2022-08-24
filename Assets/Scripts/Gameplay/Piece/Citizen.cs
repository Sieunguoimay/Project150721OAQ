using Common.Animation;
using DG.Tweening;
using UnityEngine;

namespace Gameplay.Piece
{
    [SelectionBase]
    public class Citizen : Piece
    {
        [SerializeField] private AnimatorListener animatorListener;

        private Transform _cameraTransform;
        private Animator _animator;
        public override Animator Animator => _animator ??= animatorListener.GetComponent<Animator>();

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
                transform.DORotate(target, duration).SetLink(gameObject);
            }
        }
    }
}