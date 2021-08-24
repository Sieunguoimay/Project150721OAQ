using System;
using UnityEngine;

[Serializable]
public class Player
{
    public PieceBench pieceBench;
    private Board.TileGroup tileGroup;
    public event Action<Tile, bool> OnDecisionResult = delegate(Tile tile, bool b) { };

    public Board.TileGroup TileGroup => tileGroup;

    public Player(Board.TileGroup tileGroup, PieceBench pieceBench)
    {
        this.tileGroup = tileGroup;
        this.pieceBench = pieceBench;
    }

    public virtual void MakeDecision(Board board)
    {
        Debug.Log("X");

        Tile selectedTile = null;
        bool selectedDirection = UnityEngine.Random.Range(0, 100f) > 50f;

        foreach (var t in tileGroup.tiles)
        {
            if (t.Pieces.Count > 0)
            {
                selectedTile = t;
                if (UnityEngine.Random.Range(0, 100f) > 50f)
                {
                    break;
                }
            }
        }

        Debug.Log("XX");
        pieceBench.Delay(1f, () => { InvokeOnDecisionResult(selectedTile, selectedDirection); });
        Debug.Log("XXX");
    }

    protected virtual void InvokeOnDecisionResult(Tile arg1, bool arg2) => OnDecisionResult?.Invoke(arg1, arg2);

    public virtual void ReleaseTurn()
    {
    }

    public virtual void AcquireTurn()
    {
    }
}

public class RealPlayer : Player
{
    private TileSelector tileSelector;

    public RealPlayer(Board.TileGroup tileGroup, PieceBench pieceBench, TileSelector tileSelector)
        : base(tileGroup, pieceBench)
    {
        this.tileSelector = tileSelector;
    }

    public override void MakeDecision(Board board)
    {
        tileSelector.Display(TileGroup);
    }

    public override void AcquireTurn()
    {
        this.tileSelector.OnDone = InvokeOnDecisionResult;
    }
}