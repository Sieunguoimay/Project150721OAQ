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
        int id = 0;
        foreach (var t in Tiles)
        {
            if (!t.IsConnected)
            {
                Debug.LogError("Please check the connection " + t.Id);
                return;
            }

            t.Setup();

            if (t.TileType == Tile.Type.Mandarin)
            {
                var tg = new TileGroup() {id = id++, mandarinTile = t, tiles = new List<Tile>()};
                InitializeTileGroup(ref tg);
                tileGroups.Add(tg);
            }
        }
    }

    private void InitializeTileGroup(ref TileGroup tg)
    {
        var t = tg.mandarinTile.Next;
        while (t.TileType != Tile.Type.Mandarin)
        {
            tg.tiles.Add(t);
            Debug.Log("tg " + tg.id + " " + t.name);
            t = t.Next;
        }
    }

    public struct TileGroup
    {
        public Tile mandarinTile;
        public int id;
        public List<Tile> tiles;
    }
#if UNITY_EDITOR
    private void TravelBoard(Tile tile, int steps, bool forward)
    {
        Debug.Log("Traveling " + tile.Id + " " + steps + " " + forward);
        var boardTraveller = new BoardTraveller(this,Color.black);

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