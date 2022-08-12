using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Piece;
using SNM;
using UnityEngine;
using Random = UnityEngine.Random;


namespace Gameplay.Board
{
    public class Board
    {
        [field: System.NonSerialized] public IPieceHolder[] Tiles { get; private set; }
        [field: System.NonSerialized] public TileGroup[] TileGroups { get; private set; }

        private readonly BoardTraveller _traveller = new();

        public event Action TilesChanged;

        public void SetTiles(IPieceHolder[] tiles)
        {
            Tiles = tiles;

            var mts = Tiles.Where(t => t is MandarinTile).ToArray();

            TileGroups = new TileGroup[mts.Length];

            for (var i = 0; i < mts.Length; i++)
            {
                TileGroups[i] = CreateTileGroup(mts[i]);
            }

            TilesChanged?.Invoke();
        }

        public void TearDown()
        {
        }

        public IPieceHolder GetSuccessTile(IPieceHolder tile, bool forward)
        {
            _traveller.Start(Array.IndexOf(Tiles, tile), Tiles.Length, Tiles.Length);
            _traveller.Next(forward);
            return Tiles[_traveller.CurrentIndex];
        }

        private TileGroup CreateTileGroup(IPieceHolder mt)
        {
            var tg = new TileGroup
            {
                MandarinTile = mt,
                Tiles = new List<IPieceHolder>()
            };

            _traveller.Start(Array.IndexOf(Tiles, mt), Tiles.Length, Tiles.Length);
            _traveller.Next(true);
            while (Tiles[_traveller.CurrentIndex] is not MandarinTile)
            {
                tg.Tiles.Add(Tiles[_traveller.CurrentIndex]);
                _traveller.Next(true);
            }

            return tg;
        }

        public PieceBench GetPieceBench(int index)
        {
            return new PieceBench(new PieceBench.ConfigData
            {
                PosAndRot = CalculatePieceBenchPosition(TileGroups[index]),
                spacing = 0.25f,
                perRow = 15
            });

            static PosAndRot CalculatePieceBenchPosition(TileGroup tg)
            {
                var pos1 = ((Tile) tg.Tiles[0]).transform.position;
                var pos2 = ((Tile) tg.Tiles[^1]).transform.position;
                var diff = pos2 - pos1;
                var pos = pos1 + new Vector3(diff.z, diff.y, -diff.x) * 0.5f;
                var qua = Quaternion.LookRotation(pos1 - pos, Vector3.up);
                return new PosAndRot(pos, qua);
            }
        }

        public class TileGroup
        {
            public IPieceHolder MandarinTile;
            public List<IPieceHolder> Tiles;

            public bool IsTileGroupEmpty()
            {
                return Tiles.All(t => t.Pieces.Count <= 0);
            }
        }

#if UNITY_EDITOR
        // private static void TravelBoard(Tile[] items, Tile tile, int steps, bool forward)
        // {
        //     Debug.Log("Traveling " + tile.gameObject.name + " " + steps + " " + forward);
        //     var boardTraveller = new BoardTraveller();
        //     boardTraveller.Start(Array.IndexOf(items, tile), steps, items.Length);
        //
        //     while (boardTraveller.IsTravelling)
        //     {
        //         if (!boardTraveller.Next(forward))
        //         {
        //             Debug.Log("Ended");
        //         }
        //     }
        // }

        // [ContextMenu("Connect Tiles")]
        // public void SelfConnect()
        // {
        //     Debug.Log(Tiles.Length);
        //     for (var i = 0; i < Tiles.Length; i++)
        //     {
        //         var prev = i == 0 ? Tiles.Length - 1 : i - 1;
        //         var next = i < Tiles.Length - 1 ? i + 1 : 0;
        //         Tiles[i].Connect(Tiles[prev], Tiles[next]);
        //     }
        // }

        // [ContextMenu("Test Travel X")]
        // private void TestTravel()
        // {
        //     TravelBoard(Tiles, Tiles[UnityEngine.Random.Range(0, Tiles.Length)], Random.Range(5, 10),
        //         Random.Range(0, 100) > 50);
        // }
        //
        // [ContextMenu("Popularize")]
        // private void Popularize()
        // {
        //     var mrs = GetComponentsInChildren<MeshRenderer>();
        //     foreach (var mr in mrs)
        //     {
        //         if (mr.sharedMaterial == null)
        //         {
        //             mr.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.FindAssets(mr.gameObject.name).Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault(p => Path.GetFileName(p).Equals(mr.gameObject.name + ".mat")));
        //         }
        //         else
        //         {
        //             var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.FindAssets(mr.gameObject.name).Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault(p => Path.GetFileName(p).EndsWith(mr.gameObject.name + ".psd")));
        //             mr.sharedMaterial.SetTexture("_BaseMap", tex);
        //         }
        //     }
        //
        //     AssetDatabase.SaveAssets();
        // }
#endif
    }
}