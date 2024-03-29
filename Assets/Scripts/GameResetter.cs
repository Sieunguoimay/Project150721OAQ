﻿using System;
using System.Collections.Generic;
using System.Linq;
using Manager;
using UnityEngine;

public class GameResetter
{
    private Board board;
    private PlayersManager playersManager;
    public Action OnDone = delegate { };

    public GameResetter(Board board, PlayersManager playersManager)
    {
        this.playersManager = playersManager;
        this.board = board;
    }

    public void Reset()
    {
        var citizens = new List<Piece>();
        var mandarins = new List<Piece>();
        foreach (var player in playersManager.Players)
        {
            foreach (var p in player.pieceBench.Pieces)
            {
                if (p is Citizen)
                {
                    citizens.Add(p);
                }
                else
                {
                    mandarins.Add(p);
                }
            }

            player.pieceBench.Pieces.Clear();
        }

        foreach (var tile in board.Tiles)
        {
            if (tile is MandarinTile)
            {
                tile.Grasp(mandarins, Math.Max(0, 1 - tile.Pieces.Count), p => tile.Reposition(p.transform));
            }
            else
            {
                tile.Grasp(citizens, 5, p => tile.Reposition(p.transform));
            }
        }
    }
}