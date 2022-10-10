using System;
using System.Collections.Generic;
using System.Linq;
using Common.ResolveSystem;
using Gameplay.Piece;
using SNM;
using UnityEngine;

namespace Gameplay.Board
{
    [SelectionBase]
    public class Tile : PieceContainer, RayPointer.IRaycastMiss, ISelectorTarget
    {
        [SerializeField] private float size;
        private BoxCollider _collider;
        private BoxCollider Collider => _collider ??= GetComponent<BoxCollider>();
        public event Action<Tile> OnTouched = delegate { };
        public event Action<Tile> OnOutSide = delegate { };

        public override void Setup()
        {
            base.Setup();
            RayPointer.Instance.Register(this);
            Collider.size = new Vector3(size, 0.1f, size);
        }

        public override void TearDown()
        {
            base.TearDown();
            RayPointer.Instance.Unregister(this);
        }

        #region ISelectorTarget

        public IEnumerable<CitizenToTileSelectorAdaptor> GetSelectionAdaptors() =>
            Pieces.Where(p => p is Citizen)
                .Select(p => new CitizenToTileSelectorAdaptor(p as Citizen));

        public Vector3 DisplayPos => transform.position;

        #endregion ISelectorTarget

        #region RayPointer.IRaycastTarget

        public Bounds Bounds => Collider.bounds;

        public float Size => size;

        public void OnHit(Ray ray, float distance)
        {
            OnTouched?.Invoke(this);
        }

        public void OnMiss()
        {
            OnOutSide?.Invoke(this);
        }

        #endregion RayPointer.ITarget

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            if (Collider == null) return;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Collider.center, Collider.size);
        }

        [SerializeField] private GameObject model;
        [ContextMenu("Test")]
        void Test()
        {
            Instantiate(model, transform);
        }
#endif
    }
}