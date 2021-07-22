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
    public event Action<ActionID> OnDone = delegate { };

    private ActionID actionID;

    public void Setup(Board board)
    {
        if (boardTraveller == null || boardTraveller.Board != board)
        {
            boardTraveller = new BoardTraveller(board, activeColor);
        }
    }

    public void GetReady(Tile tile)
    {
        Grasp(tile, false);
        boardTraveller.Start(tile, Citizens.Count);
        actionID = ActionID.DROPPING_IN_TURN;
    }

    public void GetReadyForTakingBackCitizens(Board.TileGroup tileGroup, List<Citizen> citizens)
    {
        int n = Mathf.Min(tileGroup.tiles.Count, citizens.Count);
        Grasp(citizens, n);
        boardTraveller.Start(tileGroup.mandarinTile, n);
        actionID = ActionID.TAKING_BACK;
    }

    private void MakeCitizenJump(Tile tile)
    {
        float delay = 0f;
        foreach (var b in Citizens)
        {
            b.Mover.EnqueueTarget(tile.SpawnRandomPosition(false));

            if (!b.Mover.IsJumpingInQueue)
            {
                b.Delay(delay, b.Mover.JumpInQueue);
                delay += 0.04f;
            }
        }
    }

    public void DropAll(bool forward)
    {
        boardTraveller.Next(forward);

        MakeCitizenJump(boardTraveller.CurrentTile);

        if (Drop())
        {
            this.Delay(0.2f, () => DropAll(forward));
        }
        else if (actionID == ActionID.DROPPING_IN_TURN)
        {
            var t = boardTraveller.CurrentTile.Success(forward);
            boardTraveller.Reset();
            if (t.Citizens.Count > 0 && t.TileType == Tile.Type.Citizen)
            {
                GetReady(t);
                DropAll(forward);
            }
            else
            {
                if (CanEatSucc(t, forward))
                {
                    Eat(t, forward, () => OnDone?.Invoke(actionID));
                }
                else
                {
                    OnDone?.Invoke(actionID);
                }
            }
        }
        else if (actionID == ActionID.TAKING_BACK)
        {
            boardTraveller.Reset();
            OnDone?.Invoke(actionID);
        }
    }

    private void Eat(Tile tile, bool forward, Action done)
    {
        var succ = tile.Success(forward);

        OnEat?.Invoke(succ);

        if (CanEatSucc(succ.Success(forward), forward))
        {
            this.Delay(0.2f, () => { Eat(succ.Success(forward), forward, done); });
        }
        else
        {
            done?.Invoke();
        }
    }

    private bool CanEatSucc(Tile tile, bool forward)
    {
        var succ = tile.Success(forward);

        return (tile.Citizens.Count == 0 && (tile.TileType == Tile.Type.Citizen) && (succ.Citizens.Count > 0));
    }

    private bool Drop()
    {
        if (Citizens.Count <= 0) return false;

        var lastIndex = Citizens.Count - 1;
        boardTraveller.CurrentTile.Grasp(Citizens[lastIndex]);
        Citizens.RemoveAt(lastIndex);

        return true;
    }


    public enum ActionID
    {
        DROPPING_IN_TURN,
        TAKING_BACK
    }
}