using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Piece
{
    public interface IPieceHolder
    {
        List<Piece> Pieces { get; }
        void Grasp(Piece piece, Action<Piece> onGrasp = null);
        void Grasp(IPieceHolder other, Action<Piece> onGrasp = null);
        void Grasp(List<Piece> otherPieces, int count = -1, Action<Piece> onGrasp = null);
        void OnGrasp(IPieceHolder other);
    }

    public class PieceHolder : IPieceHolder
    {
        [field: NonSerialized] public List<Piece> Pieces { get; } = new ();

        public virtual void Grasp(Piece piece, Action<Piece> onGrasp = null)
        {
            Pieces.Add(piece);
            onGrasp?.Invoke(piece);
        }

        public void Grasp(IPieceHolder other, Action<Piece> onGrasp = null)
        {
            foreach (var b in other.Pieces)
            {
                Grasp(b, onGrasp);
            }

            other.OnGrasp(this);
            other.Pieces.Clear();
        }

        public void Grasp(List<Piece> otherPieces, int count = -1, Action<Piece> onGrasp = null)
        {
            if (count == otherPieces.Count || count == -1)
            {
                foreach (var b in otherPieces)
                {
                    Grasp(b, onGrasp);
                }

                otherPieces.Clear();
            }
            else
            {
                var n = Mathf.Min(count, otherPieces.Count);
                for (var i = n - 1; i >= 0; i--)
                {
                    Grasp(otherPieces[i], onGrasp);
                    otherPieces.RemoveAt(i);
                }
            }
        }

        public virtual void OnGrasp(IPieceHolder whom)
        {
        }
    }
}