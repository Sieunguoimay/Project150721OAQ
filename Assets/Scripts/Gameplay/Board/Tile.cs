using System.Collections.Generic;
using System.Linq;
using Gameplay.Piece;
using UnityEngine;

namespace Gameplay.Board
{
    [SelectionBase]
    public class Tile : PieceContainer, ISelectorTarget
    {
        [SerializeField] private float size;
        private BoxCollider _collider;
        private BoxCollider Collider => _collider ??= GetComponent<BoxCollider>();

        public override void Setup()
        {
            base.Setup();
            Collider.size = new Vector3(size, 0.1f, size);
        }

        public float Size => size;

        #region ISelectorTarget

        public IEnumerable<CitizenToTileSelectorAdaptor> GetSelectionAdaptors() =>
            Pieces.Where(p => p is Citizen)
                .Select(p => new CitizenToTileSelectorAdaptor(p as Citizen));

        public Vector3 DisplayPos => transform.position;

        #endregion ISelectorTarget

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