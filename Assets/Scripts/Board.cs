using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Board : Prefab
{
    private Tile[] tiles;
    public Tile[] Tiles => tiles ?? (tiles = GetComponentsInChildren<Tile>());

    public void Setup(Action<Tile> onSelect)
    {
        foreach (var t in Tiles)
        {
            if (!t.IsConnected)
            {
                Debug.LogError("Please check the connection " + t.Id);
                return;
            }

            t.Setup();
            t.OnSelect += onSelect;
        }
    }

#if UNITY_EDITOR
    private void TravelBoard(Tile tile, int steps, bool forward)
    {
        Debug.Log("Traveling " + tile.Id + " " + steps + " " + forward);
        var boardTraveller = new BoardTraveller(this);

        boardTraveller.Start(tile, steps, forward);
        Debug.Log(boardTraveller.CurrentTile.name);

        while (boardTraveller.IsTravelling)
        {
            if (!boardTraveller.Next())
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