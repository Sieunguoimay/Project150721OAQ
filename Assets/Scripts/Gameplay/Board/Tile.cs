using System;
using SNM;
using UnityEngine;

namespace Gameplay.Board
{
    [SelectionBase]
    public class Tile : PieceContainer, RayPointer.ITarget
    {
        private BoxCollider _collider;
        private BoxCollider Collider => _collider ??= GetComponent<BoxCollider>();
        public event Action<Tile> OnTouched = delegate { };

        public override void Setup()
        {
            base.Setup();
            Main.Instance.RayPointer.Register(this);
        }

        public override void TearDown()
        {
            base.TearDown();
            Main.Instance.RayPointer.Unregister(this);
        }

        #region RayPointer.ITarget

        public Bounds Bounds => Collider.bounds;

        public void OnHit(Ray ray, float distance)
        {
            OnTouched?.Invoke(this);
        }

        #endregion

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(Bounds.center, Bounds.size);
        }
#endif
    }
}