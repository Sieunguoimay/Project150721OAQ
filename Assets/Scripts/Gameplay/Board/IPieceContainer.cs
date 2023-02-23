using System;
using System.Collections.Generic;
using Gameplay.Piece;

namespace Gameplay.Board
{
    public interface IPieceContainer
    {
        IReadOnlyList<IPiece> HeldPieces { get; }
        void AddPiece(IPiece piece);
        void RemovePiece(IPiece piece);
        void Sort(Comparison<IPiece> comparison);
        void Clear();


        public static void TransferAllPiecesOwnership(IPieceContainer @from, IPieceContainer to)
        {
            foreach (var p in @from.HeldPieces)
            {
                to.AddPiece(p);
            }

            @from.Clear();
        }
    }
}