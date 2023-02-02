using Common.Activity;
using Gameplay.Piece.Activities;
using UnityEngine;

namespace Gameplay.Piece
{
    public interface IPiece
    {
        Transform Transform { get; }
    }

    public class Piece : MonoBehaviour, IPiece
    {
        [SerializeField] private Vector3 size;
        public Transform Transform => transform;

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            var t = transform;
            Gizmos.DrawWireCube(t.position + t.up * size.y * 0.5f, size);
        }
#endif
    }
}