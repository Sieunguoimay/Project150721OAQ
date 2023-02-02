﻿using System;
using System.Collections.Generic;
using Gameplay.Piece;

namespace Gameplay.Board
{
    public interface IPieceContainer
    {
        IReadOnlyList<IPiece> HeldPieces { get; }
        void AddPiece(IPiece piece);
        void RemoveLast();
        void Sort(Comparison<IPiece> comparison);
        void Clear();
    }
}