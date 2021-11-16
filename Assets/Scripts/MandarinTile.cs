using System;
using UnityEngine;

public class MandarinTile : Tile
{
    public bool HasMandarin { get; private set; } = false;

    public override void Setup()
    {
        base.Setup();
        HasMandarin = true;
    }

    public override void OnGrasp(IPieceHolder whom)
    {
        if (HasMandarin)
        {
            HasMandarin = false;
        }
    }
}