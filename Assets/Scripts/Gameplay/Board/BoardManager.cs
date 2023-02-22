using System;
using UnityEngine;

namespace Gameplay.Board
{
    public interface IBoardManager
    {
        Board Board { get; }
        void CreateBoard(int groupNum, int tilesPerGroup);
        void DeleteBoard();
    }

    public class BoardManager : MonoControlUnitBase<BoardManager>, IBoardManager
    {
        [SerializeField] private MandarinTile mandarinTilePrefab;
        [SerializeField] private CitizenTile citizenTilePrefab;
        [field: System.NonSerialized] public Board Board { get; private set; }
        public event Action<BoardManager> BoardCreatedEvent;

        public void DeleteBoard()
        {
            foreach (var tile in Board.Tiles)
            {
                Destroy(tile.Transform.gameObject);
            }

            Board = null;
        }

        public void CreateBoard(int groupNum, int tilesPerGroup)
        {
            Board = BoardCreator.CreateBoard(groupNum, tilesPerGroup, mandarinTilePrefab, citizenTilePrefab, transform);
            BoardCreatedEvent?.Invoke(this);
        }
    }
}