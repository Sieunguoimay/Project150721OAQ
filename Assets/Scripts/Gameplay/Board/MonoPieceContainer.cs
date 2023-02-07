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
            piece.Container.SetValue(this);
        }

        public void RemovePiece(IPiece piece)
        {
            _heldPieces.Remove(piece);
            if (ReferenceEquals(piece.Container.Value, this))
            {
                piece.Container.SetValue(null);
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