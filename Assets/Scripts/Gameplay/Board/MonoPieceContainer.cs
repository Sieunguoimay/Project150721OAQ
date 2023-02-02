using System;
using System.Collections.Generic;
using Gameplay.Piece;
using UnityEngine;

namespace Gameplay.Board
{
    public class MonoPieceContainer : MonoBehaviour, IPieceContainer
    {
        private readonly List<IPiece> _heldPieces = new();
        public IReadOnlyList<IPiece> HeldPieces => _heldPieces;

        public void AddPiece(IPiece piece)
        {
            _heldPieces.Add(piece);
        }

        public void RemoveLast()
        {
            if (_heldPieces.Count > 0)
            {
                _heldPieces.RemoveAt(_heldPieces.Count - 1);
            }
        }

        public void Sort(Comparison<IPiece> comparison)
        {
            _heldPieces.Sort(comparison);
        }

        public void Clear()
        {
            _heldPieces.Clear();
        }
    }
}