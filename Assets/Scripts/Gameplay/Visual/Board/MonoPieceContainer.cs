using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Visual.Board   
{
    public class MonoPieceContainer : MonoBehaviour, IPieceContainer
    {
        private readonly List<Piece.Piece> _heldPieces = new();
        public IReadOnlyList<Piece.Piece> HeldPieces => _heldPieces;

        public void AddPieces(IEnumerable<Piece.Piece> pieces)
        {
            _heldPieces.AddRange(pieces);
        }
        public void AddPiece(Piece.Piece piece)
        {
            _heldPieces.Add(piece);
        }

        public void RemovePiece(Piece.Piece piece)
        {
            _heldPieces.Remove(piece);
        }

        public void Sort(Comparison<Piece.Piece> comparison)
        {
            _heldPieces.Sort(comparison);
        }

        public void Clear()
        {
            _heldPieces.Clear();
        }
    }
}