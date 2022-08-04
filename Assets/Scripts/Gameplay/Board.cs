using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;


namespace Gameplay
{
    public class Board : MonoBehaviour
    {
        [SerializeField] private Piece mandarinPrefab;
        [SerializeField] private Piece citizenPrefab;
        
        private Tile[] _tiles;
        public Tile[] Tiles => _tiles ??= GetComponentsInChildren<Tile>();
        [field: System.NonSerialized] public List<TileGroup> TileGroups { get; } = new();

        public void Setup()
        {
            foreach (var t in Tiles)
            {
                if (!t.IsConnected)
                {
                    return;
                }

                t.Setup();
            }
        }

        public static void InitializeTileGroup(ref TileGroup tg)
        {
            var t = tg.MandarinTile.Next;
            while (t is not MandarinTile)
            {
                tg.Tiles.Add(t);
                t = t.Next;
            }
        }

        public bool IsTileGroupEmpty(int index)
        {
            return index < TileGroups.Count && IsTileGroupEmpty(TileGroups[index]);
        }

        public static bool IsTileGroupEmpty(TileGroup tileGroup)
        {
            return tileGroup.Tiles.All(t => t.Pieces.Count <= 0);
        }

        public bool AreMandarinTilesAllEmpty()
        {
            return TileGroups.All(tg => tg.MandarinTile.Pieces.Count <= 0);
        }

        public class TileGroup
        {
            public Tile MandarinTile;
            public List<Tile> Tiles;

            public Vector3 GetForward()
            {
                var pos1 = Tiles[0].transform.position;
                var pos2 = Tiles[^1].transform.position;
                return (pos2 - pos1).normalized;
            }

            public bool TakeBackTiles(List<Piece> pieces, PieceDropper dropper)
            {
                if (pieces.Count <= 0) return false;
                
                dropper.GetReadyForTakingBackCitizens(this, pieces);
                dropper.DropAll(true);
                
                return true;

            }
        }
#if UNITY_EDITOR
        private void TravelBoard(Tile tile, int steps, bool forward)
        {
            Debug.Log("Traveling " + tile.Id + " " + steps + " " + forward);
            var boardTraveller = new BoardTraveller(this, new BoardTraveller.Config() {activeColor = Color.black});

            boardTraveller.Start(tile, steps);
            Debug.Log(boardTraveller.CurrentTile.name);

            while (boardTraveller.IsTravelling)
            {
                if (!boardTraveller.Next(forward))
                {
                    Debug.Log("Ended");
                }

                Debug.Log(boardTraveller.CurrentTile.name);
            }
        }

        [ContextMenu("Connect Tiles")]
        public void SelfConnect()
        {
            Debug.Log(Tiles.Length);
            for (int i = 0; i < Tiles.Length; i++)
            {
                int prev = i == 0 ? Tiles.Length - 1 : i - 1;
                int next = i < Tiles.Length - 1 ? i + 1 : 0;
                Tiles[i].Connect(Tiles[prev], Tiles[next]);
            }
        }

        [ContextMenu("Test Travel X")]
        private void TestTravel()
        {
            TravelBoard(Tiles[UnityEngine.Random.Range(0, Tiles.Length)], Random.Range(5, 10),
                Random.Range(0, 100) > 50);
        }
#endif
    }
}