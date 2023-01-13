using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Piece;
using UnityEngine;

namespace Gameplay.Board
{
    public class BoardManager : MonoControlUnitBase<BoardManager>
    {
        [SerializeField] private Tile mandarinTilePrefab;
        [SerializeField] private Tile citizenTilePrefab;
        [field: System.NonSerialized] public Board Board { get; private set; }

        public void ClearAll()
        {
            foreach (var t in Board.Tiles)
            {
                t.PiecesContainer.Clear();
            }
        }

        public void CreateBoard(int groupNum, int tilesPerGroup)
        {
            Board = BoardCreator.CreateBoard(groupNum, tilesPerGroup, mandarinTilePrefab, citizenTilePrefab, transform);
        }
    }
}