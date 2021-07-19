using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BunnieDropper : MonoBehaviour
{
    private List<Bunnie> bunnies = new List<Bunnie>();

    public List<Bunnie> Bunnies => bunnies;

    private BoardTraveller boardTraveller = null;

    public bool IsTravelling => boardTraveller?.IsTravelling ?? false;

    public void Setup(Board board)
    {
        if (boardTraveller == null || boardTraveller.Board != board)
        {
            boardTraveller = new BoardTraveller(board);
        }
    }

    public void Take(Tile tile)
    {
        tile.TransferTo(ref bunnies);
        foreach (var b in Bunnies)
        {
            b.transform.SetParent(transform);
            b.transform.localPosition = Vector3.zero;
        }

        boardTraveller.Start(tile, Bunnies.Count);
    }

    public void DropAll(bool forward)
    {
        boardTraveller.Next(forward);

        if (Drop())
        {
            Delay(0.2f, () => { DropAll(forward); });
        }
        else
        {
            var next = forward ? boardTraveller.CurrentTile.Next : boardTraveller.CurrentTile.Prev;
            boardTraveller.Reset();
            if (next.Bunnies.Count > 0 && next.TileType == Tile.Type.Citizen)
            {
                Take(next);
                DropAll(forward);
            }
            else
            {
                Debug.Log("done");
            }
        }
    }

    public bool Drop()
    {
        if (Bunnies.Count <= 0) return false;

        var lastIndex = Bunnies.Count - 1;
        boardTraveller.CurrentTile.Keep(Bunnies[lastIndex]);
        Bunnies.RemoveAt(lastIndex);

        return true;
    }

    private void Delay(float s, Action action) => StartCoroutine(delay(s, action));

    private IEnumerator delay(float s, Action action)
    {
        yield return new WaitForSeconds(s);
        action?.Invoke();
    }
}