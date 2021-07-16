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

    public void Take(Tile tile, Board board, bool forward)
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

        boardTraveller.Start(forward ? tile.Next : tile.Prev, Bunnies.Count - 1, forward);
    }


    public bool Drop()
    {
        if (Bunnies.Count <= 0) return false;

        var lastIndex = Bunnies.Count - 1;
        boardTraveller.CurrentTile.Keep(Bunnies[lastIndex]);
        Bunnies.RemoveAt(lastIndex);

        StartCoroutine(Delay(.2f, () =>
        {
            if (!boardTraveller.Next())
            {
            }

            if (!Drop())
            {
                if (boardTraveller.CurrentTile.Next.Bunnies.Count > 0)
                {
                    Take(boardTraveller.Forward ? boardTraveller.CurrentTile.Next : boardTraveller.CurrentTile.Prev, boardTraveller.Board, boardTraveller.Forward);
                    Drop();
                }
                else
                {
                    //reset after drop
                    boardTraveller.Reset();
                }
            }
        }));
        return true;
    }

    private IEnumerator Delay(float s, Action action)
    {
        yield return new WaitForSeconds(s);
        action?.Invoke();
    }
}