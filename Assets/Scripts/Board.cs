using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


public class Board : Prefab
{
    private Tile[] tiles;
    public Tile[] Tiles => tiles ?? (tiles = GetComponentsInChildren<Tile>());

    private List<TileGroup> tileGroups = new List<TileGroup>();
    public List<TileGroup> TileGroups => tileGroups;

    public void Setup()
    {
        var container = new GameObject("Container");
        container.transform.SetParent(transform);
        container.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

        int id = 0;
        foreach (var t in Tiles)
        {
            if (!t.IsConnected)
            {
                return;
            }

            t.Setup();

            if (t is MandarinTile)
            {
                var tg = new TileGroup() {id = id++, mandarinTile = t, tiles = new List<Tile>()};
                InitializeTileGroup(ref tg);
                tileGroups.Add(tg);

                var m = Prefab.Instantiates(Main.Instance.PrefabManager.MandarinPrefab);
                m.Setup(new Piece.ConfigData(Main.Instance.GameCommonConfig.MandarinConfigData));
                t.Grasp(m);
                t.Reposition(m.transform);
            }
            else
            {
                for (int i = 0; i < 5; i++)
                {
                    var b = Prefab.Instantiates(Main.Instance.PrefabManager.CitizenPrefab);
                    b.transform.SetParent(container.transform);
                    b.Setup(new Piece.ConfigData(Main.Instance.GameCommonConfig.CitizenConfigData));
                    t.Grasp(b);
                    t.Reposition(b.transform);
                }
            }
        }
    }

    private void InitializeTileGroup(ref TileGroup tg)
    {
        var t = tg.mandarinTile.Next;
        while (!(t is MandarinTile))
        {
            tg.tiles.Add(t);
            t = t.Next;
        }
    }

    public bool IsTileGroupEmpty(int index)
    {
        return index < TileGroups.Count && IsTileGroupEmpty(TileGroups[index]);
    }

    public static bool IsTileGroupEmpty(TileGroup tileGroup)
    {
        foreach (var t in tileGroup.tiles)
        {
            if (t.Pieces.Count > 0)
            {
                return false;
            }
        }

        return true;
    }

    public bool AreMandarinTilesAllEmpty()
    {
        foreach (var tg in TileGroups)
        {
            if (tg.mandarinTile.Pieces.Count > 0)
            {
                return false;
            }
        }

        return true;
    }

    public struct TileGroup
    {
        public Tile mandarinTile;
        public int id;
        public List<Tile> tiles;

        public Vector3 GetForward()
        {
            var pos1 = tiles[0].transform.position;
            var pos2 = tiles[tiles.Count - 1].transform.position;
            return (pos2 - pos1).normalized;
        }
    }
#if UNITY_EDITOR
    private void TravelBoard(Tile tile, int steps, bool forward)
    {
        Debug.Log("Traveling " + tile.Id + " " + steps + " " + forward);
        var boardTraveller = new BoardTraveller(this, Color.black);

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
        TravelBoard(Tiles[UnityEngine.Random.Range(0, Tiles.Length)], Random.Range(5, 10), Random.Range(0, 100) > 50);
    }
#endif
}