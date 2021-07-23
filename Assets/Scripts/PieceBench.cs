using System.Collections.Generic;
using UnityEngine;

public class PieceBench : PieceContainer
{
    private List<Mandarin> mandarins = new List<Mandarin>();

    public List<Mandarin> Mandarins => mandarins;

    public override void Grasp(Piece piece)
    {
        if (piece is Mandarin)
        {
            Mandarins.Add(piece as Mandarin);
        }
        else
        {
            base.Grasp(piece);
        }
    }
}