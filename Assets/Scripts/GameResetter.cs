using System;
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
        int playerNum = playersManager.Players.Length;
        int playerIndex = 0;
        var mandarins = new Piece[playersManager.Players.Length];
        int i = 0;

        foreach (var p in playersManager.Players)
        {
            for (int j = p.pieceBench.Pieces.Count - 1; j >= 0; j--)
            {
                var pi = p.pieceBench.Pieces[j];
                if (pi is Mandarin)
                {
                    mandarins[i++] = pi;
                    p.pieceBench.Pieces.RemoveAt(j);
                }
            }
        }

        for (int j = 0; j < mandarins.Length; j++)
        {
            board.TileGroups[j].mandarinTile.Grasp(mandarins[j]);
            board.TileGroups[j].mandarinTile.Reposition(mandarins[j].transform);
        }

        foreach (var tg in board.TileGroups)
        {
            bool mandarin = false;
            foreach (var t in tg.tiles)
            {
                var b = playersManager.Players[playerIndex].pieceBench;
                if (t.TileType != Tile.Type.Mandarin)
                {
                    var ps = b.Pieces;
                    var n = GameCommonConfig.PieceNumPerTile;
                    if (ps.Count >= n)
                    {
                        t.Grasp(ps, n, true);
                    }
                    else if (ps.Count > 0)
                    {
                        t.Grasp(ps, ps.Count, true);

                        if (playerIndex < playerNum)
                        {
                            playerIndex++;
                        }
                        else
                        {
                            Debug.LogError("GameResetter:Reset:ERROR playerIndex exceed playerNum");
                            break;
                        }

                        ps = playersManager.Players[playerIndex].pieceBench.Pieces;
                        t.Grasp(ps, n - ps.Count, true);
                    }
                    else
                    {
                        if (playerIndex < playerNum)
                        {
                            playerIndex++;
                        }
                        else
                        {
                            Debug.LogError("GameResetter:Reset:ERROR playerIndex exceed playerNum");
                            break;
                        }

                        ps = playersManager.Players[playerIndex].pieceBench.Pieces;
                        t.Grasp(ps, n, true);
                    }
                }
            }
        }
    }
}