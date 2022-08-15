using System;
using System.Collections.Generic;
using System.Linq;
using Common.ResolveSystem;
using Gameplay.Piece;
using UnityEngine;

namespace Gameplay.Board
{
    public class BoardManager : MonoBehaviour
    {
        [SerializeField] private BoardMetaData[] boardGOs;
        public Board Board { get; } = new();

        public void SetBoardByTileGroupNum(int tileGroupNum, int tilesPerGroup)
        {
            Board.SetTiles(CreateBoard(tileGroupNum, tilesPerGroup).Select(t => t as IPieceHolder).ToArray());
        }

        private IEnumerable<Tile> CreateBoard(int groupNum, int tilesPerGroup)
        {
            var prefab = MapToBoardPrefab();
            var tiles = Instantiate(prefab, transform).GetComponentsInChildren<Tile>();
            foreach (var tile in tiles)
            {
                tile.Setup();
            }

            return tiles;

            GameObject MapToBoardPrefab()
            {
                return boardGOs.FirstOrDefault(m => m.groupNum == groupNum && m.tilesPerGroup == tilesPerGroup)?.prefab;
            }
        }

        [Serializable]
        private class BoardMetaData
        {
            public int groupNum;
            public int tilesPerGroup;
            public GameObject prefab;
        }
    }
}