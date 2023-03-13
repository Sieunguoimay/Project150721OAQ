using Gameplay.Helpers;
using UnityEngine;

namespace Gameplay.Visual.Board
{
    [SelectionBase]
    public class TileVisual : MonoPieceContainer
    {
        public int TileIndex { get; private set; }

        public void SetIndex(int tileIndex)
        {
            TileIndex = tileIndex;
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }

        public int GetPiecesCount()
        {
            return HeldPieces.Count;
        }

        public virtual int GetNumTakenGridCells()
        {
            return GetPiecesCount();
        }
        
#if UNITY_EDITOR
        [SerializeField] private bool drawGizmos;
        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.up * .05f, new Vector3(1, 0.1f, 1));
        }
#endif
    }
}