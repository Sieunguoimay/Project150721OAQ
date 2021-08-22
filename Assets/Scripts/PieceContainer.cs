using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class PieceContainer : MonoBehaviour
{
    private const int maxPiecesSupported = 25;
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

    public void Grasp(List<Piece> otherPieces, int count = -1, bool reposition = false)
    {
        if (count == otherPieces.Count || count == -1)
        {
            foreach (var b in otherPieces)
            {
                Grasp(b);
                if (reposition)
                {
                    Reposition(b.transform);
                }
            }

            otherPieces.Clear();
        }
        else
        {
            int n = Mathf.Min(count, otherPieces.Count);
            for (int i = n - 1; i >= 0; i--)
            {
                Grasp(otherPieces[i]);
                if (reposition)
                {
                    Reposition(otherPieces[i].transform);
                }

                otherPieces.RemoveAt(i);
            }
        }
    }

    public void Reposition(Transform t)
    {
        t.position = SpawnPositionInUnityUnit(Mathf.Max(0, Pieces.Count - 1), false);
    }

    public Vector3 SpawnRandomPosition(bool local = true)
    {
        var randomPos = Random.insideUnitCircle * 0.2f;
        var offset = transform.up + new Vector3(randomPos.x, 0f, randomPos.y);
        var pos = Vector3.Scale(offset, transform.localScale);
        if (!local)
        {
            pos = transform.TransformPoint(pos);
        }

        return pos;
    }

    public Vector3 SpawnPositionInUnityUnit(int index, bool local = false, float size = 0.6f)
    {
        int a = 0;
        int b = 0;
        var order = new[] {(1, 2), (2, 1), (2, -1), (1, -2), (-1, -2), (-2, 1), (-1, 2)};
        var orderNum = order.Length;
        var existing = new List<(int, int)>();
        int oi = 0;
        for (int i = 0; i < index + 1; i++)
        {
            var o = order[oi++];

            int x = GetValue(a, b, o.Item1);
            int y = GetValue(a, b, o.Item2);

            if (!CheckExisting(x, y, existing))
            {
                existing.Add((x, y));
            }
            else
            {
                i--;
            }

            if (oi == orderNum - 1)
            {
                oi = 0;
                if (b >= a)
                {
                    a++;
                    b = 0;
                    oi = 0;
                }
                else
                {
                    b++;
                }
            }
        }

        var pos = new Vector3();
        if (existing.Count > 0)
        {
            var e = existing[existing.Count - 1];
            int n = (int) Mathf.Sqrt(maxPiecesSupported);
            float scale = 1f / n;
            pos.x = e.Item1 * scale * size;
            pos.z = e.Item2 * scale * size;
        }

        if (!local)
        {
            pos = transform.TransformPoint(pos);
        }

        return pos;
    }

    private int GetValue(int a, int b, int o)
    {
        var sign = Mathf.Sign(o);
        if (Mathf.Abs(o) == 2)
        {
            return (int) (a * sign);
        }
        else if (Mathf.Abs(o) == 1)
        {
            return (int) (b * sign);
        }

        return 0;
    }

    private bool CheckExisting(int a, int b, List<(int, int)> existingList)
    {
        foreach (var e in existingList)
        {
            if (e.Item1 == a && e.Item2 == b)
            {
                return true;
            }
        }

        return false;
    }
#if UNITY_EDITOR
    [SerializeField] private int input = 1;
    [ContextMenu("Test Get Position")]
    private void TestGetPosition()
    {
        SpawnPositionInUnityUnit(input);
    }
#endif
}