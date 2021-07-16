using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BunnieDropper : MonoBehaviour
{
    private List<Bunnie> bunnies = new List<Bunnie>();

    public List<Bunnie> Bunnies => bunnies;

    private BoardTraveller boardTraveller = null;

    public void Take(Tile tile, Board board)
    {
        tile.TransferTo(ref bunnies);
        foreach (var b in Bunnies)
        {
            b.transform.SetParent(transform);
            b.transform.localPosition = Vector3.zero;
        }

        if (boardTraveller == null || boardTraveller.Board != board)
        {
            boardTraveller = new BoardTraveller(board);
        }
        boardTraveller.Start(tile.Next, Bunnies.Count, true);
    }

    public void Drop()
    {
        if (Bunnies.Count <= 0) return;

        var lastIndex = Bunnies.Count - 1;
        boardTraveller.CurrentTile.Keep(Bunnies[lastIndex]);
        Bunnies.RemoveAt(lastIndex);
        StartCoroutine(Delay(.2f, ()=>
        {
            boardTraveller.Next();
            Drop();
        }));
    }

    private IEnumerator Delay(float s, Action action)
    {
        yield return new WaitForSeconds(s);
        action?.Invoke();
    }
}