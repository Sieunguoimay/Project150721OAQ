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

        public void DeleteBoard()
        {
            foreach (var tile in Board.Tiles)
            {
                Destroy(tile.gameObject);
            }

            Board = null;
        }

        public void CreateBoard(int groupNum, int tilesPerGroup)
        {
            Board = BoardCreator.CreateBoard(groupNum, tilesPerGroup, mandarinTilePrefab, citizenTilePrefab, transform);
        }
    }
}