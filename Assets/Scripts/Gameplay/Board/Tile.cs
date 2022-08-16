using System;
using SNM;
using UnityEngine;

namespace Gameplay.Board
{
    [SelectionBase]
    public class Tile : PieceContainer, RayPointer.ITarget
    {
        [SerializeField] private float size;
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

        public float Size => size;

        public void OnHit(Ray ray, float distance)
        {
            OnTouched?.Invoke(this);
        }

        #endregion

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Collider.center, Collider.size);
        }
#endif
    }
}