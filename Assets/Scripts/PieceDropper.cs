using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PieceDropper : PieceHolder
{
    [Serializable]
    public class Config
    {
        [SerializeField] private Color activeColor;
        public Color ActiveColor => activeColor;
    }

    private Config config;

    private BoardTraveller boardTraveller = null;
    public bool IsTravelling => boardTraveller?.IsTravelling ?? false;

    // public bool IsTerminated => Pieces.Count == 0 && boardTraveller.CurrentTile.Next.Pieces.Count == 0 &&
    //                             (boardTraveller.CurrentTile.Next.Next.Pieces.Count == 0 ||
    //                              boardTraveller.CurrentTile.Next.Next.TileType == Tile.Type.Mandarin);

    public event Action<PieceContainer> OnEat = delegate { };
    public event Action<ActionID> OnDone = delegate { };

    private ActionID actionID;
    private bool forward;


    public void Setup(Board board, Config config)
    {
        this.config = config;
        if (boardTraveller == null || boardTraveller.Board != board)
        {
            boardTraveller = new BoardTraveller(board, config.ActiveColor);
        }
    }

    public void GetReady(Tile tile)
    {
        Grasp(tile);
        boardTraveller.Start(tile, Pieces.Count);
        actionID = ActionID.DROPPING_IN_TURN;
    }

    public void GetReadyForTakingBackCitizens(Board.TileGroup tileGroup, List<Piece> citizens)
    {
        var cs = citizens.Where(c => c is Citizen).ToList();

        int n = Mathf.Min(tileGroup.tiles.Count, cs.Count);
        Grasp(cs, n, p => citizens.Remove(p));

        boardTraveller.Start(tileGroup.mandarinTile, Pieces.Count);
        actionID = ActionID.TAKING_BACK;
    }

    public void DropAll(bool forward)
    {
        this.forward = forward;
        float delay = 0f;
        for (int i = 0; i < Pieces.Count; i++)
        {
            boardTraveller.Next(forward);

            for (int j = 0; j < Pieces.Count - i; j++)
            {
                var b = Pieces[i + j];
                b.Mover.EnqueueTarget(new Mover.JumpTarget
                {
                    target = boardTraveller.CurrentTile.SpawnPositionInUnityUnit(Math.Max(0, boardTraveller.CurrentTile.Pieces.Count - 1), false),
                    flag = (i == Pieces.Count - 1) ? 2 : (j == 0 ? 1 : 0),
                    onDone = OnJumpDone
                });

                if (i == 0)
                {
                    b.Delay(delay, b.Mover.JumpInQueue);
                    delay += 0.08f;
                }
            }

            boardTraveller.CurrentTile.Grasp(Pieces[i]);
        }

        Pieces.Clear();
    }

    public void OnJumpDone(Mover last, int flag)
    {
        if (flag == 2)
        {
            OnDropAllDone();
        }
    }

    private void OnDropAllDone()
    {
        if (actionID == ActionID.DROPPING_IN_TURN)
        {
            var t = boardTraveller.CurrentTile.Success(forward);
            boardTraveller.Reset();

            if (t.Pieces.Count > 0 && t.TileType == Tile.Type.Citizen)
            {
                GetReady(t);
                Main.Instance.Delay(.3f, () =>
                {
                    DropAll(forward);
                });
            }
            else
            {
                if (CanEatSucc(t, forward))
                {
                    Eat(t, forward, () => { OnDone?.Invoke(actionID); });
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
            Main.Instance.Delay(0.2f, () => { Eat(succ.Success(forward), forward, done); });
        }
        else
        {
            done?.Invoke();
        }
    }

    private bool CanEatSucc(Tile tile, bool forward)
    {
        var succ = tile.Success(forward);

        return (tile.Pieces.Count == 0 && (tile.TileType == Tile.Type.Citizen) && (succ.Pieces.Count > 0));
    }

    public enum ActionID
    {
        DROPPING_IN_TURN,
        TAKING_BACK
    }
}