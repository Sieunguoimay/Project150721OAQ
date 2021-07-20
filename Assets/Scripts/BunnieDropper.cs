using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BunnieDropper : CitizenContainer
{
    private BoardTraveller boardTraveller = null;
    public bool IsTravelling => boardTraveller?.IsTravelling ?? false;
    public event Action<CitizenContainer> OnEat = delegate { };
    public event Action OnDone = delegate { };

    public void Setup(Board board)
    {
        if (boardTraveller == null || boardTraveller.Board != board)
        {
            boardTraveller = new BoardTraveller(board);
        }
    }

    public void GetReady(Tile tile)
    {
        Grasp(tile.Bunnies);
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
            var t = boardTraveller.CurrentTile.Success(forward);
            boardTraveller.Reset();
            if (t.Bunnies.Count > 0 && t.TileType == Tile.Type.Citizen)
            {
                GetReady(t);
                DropAll(forward);
            }
            else
            {
                Eat(t, forward);
                OnDone?.Invoke();
                Debug.Log("done");
            }
        }
    }

    private void Eat(Tile tile, bool forward)
    {
        var succ = tile.Success(forward);

        if (tile.Bunnies.Count == 0 && (tile.TileType == Tile.Type.Citizen) && (succ.Bunnies.Count > 0))
        {
            OnEat?.Invoke(succ);
            Delay(0.2f, () => { Eat(succ.Success(forward), forward); });
        }
    }

    public bool Drop()
    {
        if (Bunnies.Count <= 0) return false;

        var lastIndex = Bunnies.Count - 1;
        boardTraveller.CurrentTile.Grasp(Bunnies[lastIndex]);
        boardTraveller.CurrentTile.Localize(Bunnies[lastIndex].transform);
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