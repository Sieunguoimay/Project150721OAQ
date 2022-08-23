using System;
using Gameplay.Piece;

namespace Gameplay.Board
{
    public class Board
    {
        [field: System.NonSerialized] public IPieceHolder[] Tiles { get; private set; }
        [field: System.NonSerialized] public TileGroup[] TileGroups { get; private set; }

        private readonly BoardTraveller _traveller = new();

        public void SetGroups(TileGroup[] groups)
        {
            TileGroups = groups;
            Tiles = new IPieceHolder[TileGroups.Length * TileGroups[0].Tiles.Length + TileGroups.Length];

            var index = 0;
            foreach (var tg in TileGroups)
            {
                Tiles[index++] = tg.MandarinTile;
                foreach (var t in tg.Tiles)
                {
                    Tiles[index++] = t;
                }
            }
        }

        public IPieceHolder GetSuccessTile(IPieceHolder tile, bool forward)
        {
            _traveller.Start(Array.IndexOf(Tiles, tile), Tiles.Length, Tiles.Length);
            _traveller.Next(forward);
            return Tiles[_traveller.CurrentIndex];
        }

        public class TileGroup
        {
            public IPieceHolder MandarinTile;
            public IPieceHolder[] Tiles;
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