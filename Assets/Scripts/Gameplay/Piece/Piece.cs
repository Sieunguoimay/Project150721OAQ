using System;
using Framework.Entities.Variable;
using Gameplay.Board;
using UnityEngine;

namespace Gameplay.Piece
{

    public interface IPiece
    {
        IVariable<IPieceContainer> Container { get; }
        Transform Transform { get; }
    }

    public class Piece : MonoBehaviour, IPiece
    {
        [SerializeField] private Vector3 size;
        public IVariable<IPieceContainer> Container { get; } = new Variable<IPieceContainer>();
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