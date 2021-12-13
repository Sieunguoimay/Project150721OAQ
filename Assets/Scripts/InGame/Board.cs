using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


public class Board : MonoBehaviour
{
    [SerializeField] private GameObject mandarinPrefab;
    [SerializeField] private GameObject citizenPrefab;
    private Tile[] _tiles;
    public Tile[] Tiles => _tiles ?? (_tiles = GetComponentsInChildren<Tile>());
    public List<TileGroup> TileGroups { get; } = new List<TileGroup>();

    public void Setup()
    {
        var container = new GameObject("Container");
        container.transform.SetParent(transform);
        container.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

        foreach (var t in Tiles)
        {
            if (!t.IsConnected)
            {
                return;
            }

            t.Setup();

            if (t is MandarinTile)
            {
                var tg = new TileGroup() {MandarinTile = t, Tiles = new List<Tile>()};
                InitializeTileGroup(ref tg);
                TileGroups.Add(tg);

                var m = Instantiate(mandarinPrefab).GetComponent<Mandarin>();
                m.Setup();
                t.Grasp(m);
                t.Reposition(m.transform);
            }
            else
            {
                for (int i = 0; i < 5; i++)
                {
                    var b = Instantiate(citizenPrefab).GetComponent<Citizen>();
                    b.transform.SetParent(container.transform);
                    b.Setup();
                    t.Grasp(b);
                    t.Reposition(b.transform);
                }
            }
        }
    }

    private void InitializeTileGroup(ref TileGroup tg)
    {
        var t = tg.MandarinTile.Next;
        while (!(t is MandarinTile))
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
        foreach (var t in tileGroup.Tiles)
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
            if (tg.MandarinTile.Pieces.Count > 0)
            {
                return false;
            }
        }

        return true;
    }

    public class TileGroup
    {
        public Tile MandarinTile;
        public List<Tile> Tiles;

        public Vector3 GetForward()
        {
            var pos1 = Tiles[0].transform.position;
            var pos2 = Tiles[Tiles.Count - 1].transform.position;
            return (pos2 - pos1).normalized;
        }

        public bool TakeBackTiles(List<Piece> pieces, PieceDropper dropper)
        {
            if (pieces.Count > 0)
            {
                dropper.GetReadyForTakingBackCitizens(this, pieces);
                dropper.DropAll(true);
                return true;
            }

            return false;
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
        TravelBoard(Tiles[UnityEngine.Random.Range(0, Tiles.Length)], Random.Range(5, 10), Random.Range(0, 100) > 50);
    }
#endif
}