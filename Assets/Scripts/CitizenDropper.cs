using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CitizenDropper : CitizenContainer
{
    [SerializeField] private Color activeColor;
    private BoardTraveller boardTraveller = null;
    public bool IsTravelling => boardTraveller?.IsTravelling ?? false;
    public event Action<CitizenContainer> OnEat = delegate { };
    public event Action OnDone = delegate { };

    public void Setup(Board board)
    {
        if (boardTraveller == null || boardTraveller.Board != board)
        {
            boardTraveller = new BoardTraveller(board, activeColor);
        }
    }

    public void GetReady(Tile tile)
    {
        Grasp(tile.Bunnies, false);
        boardTraveller.Start(tile, Bunnies.Count);
    }

    public void DropAllCitizen(bool forward)
    {
        DropAll(forward);
    }

    private void MakeCitizenJump(Tile tile)
    {
        float delay = 0f;
        foreach (var b in Bunnies)
        {
            b.Mover.EnqueueTarget(tile.SpawnRandomPosition(false));

            if (!b.Mover.IsJumpingInQueue)
            {
                b.Delay(delay, b.Mover.JumpInQueue);
                delay += 0.02f;
            }
        }
    }

    private void DropAll(bool forward)
    {
        boardTraveller.Next(forward);

        MakeCitizenJump(boardTraveller.CurrentTile);

        if (Drop())
        {
            Delay(0.2f, () => DropAll(forward));
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
                if (CanEatSucc(t, forward))
                {
                    Eat(t, forward, OnDone);
                }
                else
                {
                    OnDone?.Invoke();
                }
            }
        }

    }

    private void Eat(Tile tile, bool forward, Action done)
    {
        var succ = tile.Success(forward);

        OnEat?.Invoke(succ);

        if (CanEatSucc(succ.Success(forward), forward))
        {
            Delay(0.2f, () => { Eat(succ.Success(forward), forward, done); });
        }
        else
        {
            done?.Invoke();
        }
    }

    private bool CanEatSucc(Tile tile, bool forward)
    {
        var succ = tile.Success(forward);

        return (tile.Bunnies.Count == 0 && (tile.TileType == Tile.Type.Citizen) && (succ.Bunnies.Count > 0));
    }

    public bool Drop()
    {
        if (Bunnies.Count <= 0) return false;

        var lastIndex = Bunnies.Count - 1;
        boardTraveller.CurrentTile.Grasp(Bunnies[lastIndex]);
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