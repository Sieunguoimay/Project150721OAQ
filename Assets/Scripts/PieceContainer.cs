using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PieceContainer : MonoBehaviour
{
    public List<Piece> Pieces { get; private set; } = new List<Piece>();

    public event Action<PieceContainer> OnEmpty = delegate { };

    public virtual void Grasp(Piece piece)
    {
        Pieces.Add(piece);
    }

    public void Grasp(PieceContainer other, bool localize = true)
    {
        foreach (var b in other.Pieces)
        {
            Grasp(b);
            if (localize)
            {
                Reposition(b.transform);
            }
        }

        other.Pieces.Clear();
        other.OnEmpty?.Invoke(other);
    }

    public void Grasp(List<Piece> otherPieces, int count = -1)
    {
        if (count == otherPieces.Count || count == -1)
        {
            foreach (var b in otherPieces)
            {
                Grasp(b);
            }

            otherPieces.Clear();
        }
        else
        {
            int n = Mathf.Min(count, otherPieces.Count);
            for (int i = n - 1; i >= 0; i--)
            {
                Grasp(otherPieces[i]);
                otherPieces.RemoveAt(i);
            }
        }
    }

    public void Reposition(Transform t)
    {
        t.position = SpawnRandomPosition(false);
    }

    public Vector3 SpawnRandomPosition(bool local = true)
    {
        var randomPos = Random.insideUnitCircle * 0.2f;
        var offset = transform.up * 0.5f + new Vector3(randomPos.x, 0f, randomPos.y);
        var pos = Vector3.Scale(offset, transform.localScale);
        if (!local)
        {
            pos = transform.TransformPoint(pos);
        }

        return pos;
    }
}