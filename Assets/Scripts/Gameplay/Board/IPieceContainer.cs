using System;
using System.Collections.Generic;

namespace Gameplay.Board
{
    public interface IPieceContainer
    {
        IReadOnlyList<Piece.Piece> HeldPieces { get; }
        void AddPiece(Piece.Piece piece);
        void RemoveLast();
        void Sort(Comparison<Piece.Piece> comparison);
        void Clear();
    }
}