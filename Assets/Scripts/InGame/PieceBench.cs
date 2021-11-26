using System;
using System.Collections.Generic;
using SNM;
using UnityEngine;

public class PieceBench : PieceHolder
{
    
    [Serializable]
    public struct ConfigData
    {
        public Placement placement;
        public float spacing;
        public int perRow;
    }
    
    private ConfigData configData;

    public int MandarinCount { get; private set; } = 0;

    public PieceBench(ConfigData configData)
    {
        this.configData = configData;
    }

    public override void Grasp(Piece piece, Action<Piece> onGrasp = null)
    {
        if (piece is Mandarin)
        {
            MandarinCount++;
        }

        base.Grasp(piece, onGrasp);
    }

    public Placement[] GetPlacements(int n)
    {
        var placements = new Placement[n];
        var dirX = configData.placement.Rotation * Vector3.right;
        var dirY = configData.placement.Rotation * Vector3.forward;
        var existing = Pieces.Count;
        for (int i = 0; i < n; i++)
        {
            var x = (existing + i) % configData.perRow;
            var y = (existing + i) / configData.perRow;
            var offsetX = configData.spacing * x;
            var offsetY = configData.spacing * y;
            placements[i] = new Placement(configData.placement.Position + dirX * offsetX + dirY * offsetY, configData.placement.Rotation);
        }

        return placements;
    }

    public Placement GetPlacement(int index)
    {
        var dirX = configData.placement.Rotation * Vector3.right;
        var dirY = configData.placement.Rotation * Vector3.forward;
        var x = index % configData.perRow;
        var y = index / configData.perRow;
        var offsetX = configData.spacing * x;
        var offsetY = configData.spacing * y;
        return new Placement(configData.placement.Position + dirX * offsetX + dirY * offsetY, configData.placement.Rotation);
    }

    public Placement GetMandarinPlacement(int index)
    {
        var dirX = configData.placement.Rotation * Vector3.left;
        var dirY = configData.placement.Rotation * Vector3.forward;
        var y = index;
        var offsetX = configData.spacing;
        var offsetY = configData.spacing * y;
        return new Placement(configData.placement.Position + dirX * offsetX + dirY * offsetY, configData.placement.Rotation);
    }

}