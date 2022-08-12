using System;
using System.Linq;
using Common.ResolveSystem;
using Gameplay.Piece;
using UnityEngine;

namespace Gameplay.Board
{
    public class BoardManager : MonoBehaviour
    {
        [SerializeField] private GameObject[] boardGOs;

        public Board Board { get; } = new();

        private void OnDestroy()
        {
            foreach (var bgo in boardGOs)
            {
                foreach (var t in bgo.GetComponentsInChildren<Tile>())
                {
                    t.TearDown();
                }
            }
        }

        public void ChangeBoard(int index)
        {
            if (Board.Tiles != null)
            {
                foreach (var t in Board.Tiles)
                {
                    (t as Tile)?.TearDown();
                }
            }

            var boardGO = boardGOs[index % boardGOs.Length];
            var tiles = boardGO.GetComponentsInChildren<Tile>();
            foreach (var t in tiles)
            {
                t.Setup();
            }

            Board.SetTiles(tiles.Select(t => t as IPieceHolder).ToArray());
        }

#if UNITY_EDITOR
        [ContextMenu("ChangeBoard")]
        private void ChangeBoard() => ChangeBoard(_testIndex += 1);

        private int _testIndex = 0;
#endif
    }
}