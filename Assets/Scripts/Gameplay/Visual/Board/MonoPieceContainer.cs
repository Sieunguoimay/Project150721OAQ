using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Visual.Board
{
    public class MonoPieceContainer : MonoBehaviour, IPieceContainer
    {
        private readonly SimplePieceContainer _container = new();
        
        public IReadOnlyList<Piece.PieceVisual> HeldPieces => _container.HeldPieces;
        public void AddPieces(IEnumerable<Piece.PieceVisual> pieces) => _container.AddPieces(pieces);
        public void AddPiece(Piece.PieceVisual pieceVisual) => _container.AddPiece(pieceVisual);
        public void RemovePiece(Piece.PieceVisual pieceVisual) => _container.RemovePiece(pieceVisual);
        public void Sort(Comparison<Piece.PieceVisual> comparison) => _container.Sort(comparison);
        public void Clear() => _container.Clear();
    }

    public class SimplePieceContainer : IPieceContainer
    {
        private readonly List<Piece.PieceVisual> _heldPieces = new();
        public IReadOnlyList<Piece.PieceVisual> HeldPieces => _heldPieces;

        public void AddPieces(IEnumerable<Piece.PieceVisual> pieces)
        {
            _heldPieces.AddRange(pieces);
        }

        public void AddPiece(Piece.PieceVisual pieceVisual)
        {
            _heldPieces.Add(pieceVisual);
        }

        public void RemovePiece(Piece.PieceVisual pieceVisual)
        {
            _heldPieces.Remove(pieceVisual);
        }

        public void Sort(Comparison<Piece.PieceVisual> comparison)
        {
            _heldPieces.Sort(comparison);
        }

        public void Clear()
        {
            _heldPieces.Clear();
        }
    }
}