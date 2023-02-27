using System;
using System.Collections.Generic;
using Gameplay.Piece;
using UnityEngine;

namespace Gameplay.Board
{
    public class MonoPieceContainer : MonoBehaviour, IPieceContainer
    {
        private readonly List<Piece.Piece> _heldPieces = new();
        public IReadOnlyList<Piece.Piece> HeldPieces => _heldPieces;

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