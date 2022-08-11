using System;
using System.Collections.Generic;
using Gameplay;
using Gameplay.Board;
using Gameplay.Piece;
using SNM;
using UnityEngine;

public class PieceBench : PieceHolder
{
    [Serializable]
    public struct ConfigData
    {
        public PosAndRot PosAndRot;
        public float spacing;
        public int perRow;
    }
    
    private readonly ConfigData _config;

    public int MandarinCount { get; private set; } = 0;

    public ConfigData Config => _config;

    public PieceBench(ConfigData config)
    {
        _config = config;
    }

    public override void Grasp(Piece piece, Action<Piece> onGrasp = null)
    {
        if (piece is Mandarin)
        {
            MandarinCount++;
        }

        base.Grasp(piece, onGrasp);
    }

    public PosAndRot GetPosAndRot(int index)
    {
        var dirX = _config.PosAndRot.Rotation * Vector3.right;
        var dirY = _config.PosAndRot.Rotation * Vector3.forward;
        var x = index % _config.perRow;
        var y = index / _config.perRow;
        var offsetX = _config.spacing * x;
        var offsetY = _config.spacing * y;
        return new PosAndRot(_config.PosAndRot.Position + dirX * offsetX + dirY * offsetY, _config.PosAndRot.Rotation);
    }

    public PosAndRot GetMandarinPosAndRot(int index)
    {
        var dirX = _config.PosAndRot.Rotation * Vector3.left;
        var dirY = _config.PosAndRot.Rotation * Vector3.forward;
        var y = index;
        var offsetX = _config.spacing;
        var offsetY = _config.spacing * y;
        return new PosAndRot(_config.PosAndRot.Position + dirX * offsetX + dirY * offsetY, _config.PosAndRot.Rotation);
    }

}