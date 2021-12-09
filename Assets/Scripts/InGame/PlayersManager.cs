using System;
using System.Collections.Generic;
using SNM;
using UnityEngine;

public class PlayersManager
{
    public class StateData
    {
        public int Turn = 0;
    }

    private readonly StateData _state = new StateData();

    private Player[] _players;

    public Player[] Players => _players;
    public Player CurrentPlayer => Players[_state.Turn];

    public void Setup(List<Board.TileGroup> tileGroups, TileSelector tileSelector)
    {
        int n = tileGroups.Count;
        _players = new Player[n];

        for (int i = 0; i < n; i++)
        {
            var pieceBench = new PieceBench(new PieceBench.ConfigData
            {
                LinearTransform = CalculatePlayerPosition(tileGroups[i]),
                spacing = 0.25f,
                perRow = 15
            });

            if (i == n - 1)
            {
                _players[i] = new RealPlayer(tileGroups[i], pieceBench, tileSelector);
            }
            else
            {
                _players[i] = new Player(tileGroups[i], pieceBench);
            }
        }

        CurrentPlayer.AcquireTurn();
    }

    private LinearTransform CalculatePlayerPosition(Board.TileGroup tg)
    {
        var pos1 = tg.Tiles[0].transform.position;
        var pos2 = tg.Tiles[tg.Tiles.Count - 1].transform.position;
        var diff = pos2 - pos1;
        var pos = pos1 + new Vector3(diff.z, diff.y, -diff.x) * 0.5f;
        var qua = Quaternion.LookRotation(pos1 - pos, Vector3.up);
        return new LinearTransform(pos, qua);
    }

    public void ChangePlayer()
    {
        CurrentPlayer.ReleaseTurn();
        _state.Turn = (_state.Turn + 1) % Players.Length;
        CurrentPlayer.AcquireTurn();
    }

}