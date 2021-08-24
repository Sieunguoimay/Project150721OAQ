using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public interface IPieceHolder
{
    List<Piece> Pieces { get; }
    void Grasp(Piece piece, Action<Piece> onGrasp = null);
    void Grasp(IPieceHolder other, Action<Piece> onGrasp = null);
    void Grasp(List<Piece> otherPieces, int count = -1, Action<Piece> onGrasp = null);
}

public class PieceHolder : IPieceHolder
{
    public const int MaxPiecesSupported = 25;

    public List<Piece> Pieces { get; } = new List<Piece>();

    public void Grasp(Piece piece, Action<Piece> onGrasp = null)
    {
        Pieces.Add(piece);
        onGrasp?.Invoke(piece);
    }

    public void Grasp(IPieceHolder other, Action<Piece> onGrasp = null)
    {
        foreach (var b in other.Pieces)
        {
            Grasp(b, onGrasp);
        }

        other.Pieces.Clear();
    }

    public void Grasp(List<Piece> otherPieces, int count = -1, Action<Piece> onGrasp = null)
    {
        if (count == otherPieces.Count || count == -1)
        {
            foreach (var b in otherPieces)
            {
                Grasp(b, onGrasp);
            }

            otherPieces.Clear();
        }
        else
        {
            int n = Mathf.Min(count, otherPieces.Count);
            for (int i = n - 1; i >= 0; i--)
            {
                Grasp(otherPieces[i], onGrasp);
                otherPieces.RemoveAt(i);
            }
        }
    }
}

public class PieceContainer : MonoBehaviour, IPieceHolder
{
    #region IPieceHolder

    private PieceHolder pieceHolder = new PieceDropper();
    public List<Piece> Pieces => pieceHolder.Pieces;
    public virtual void Grasp(Piece piece, Action<Piece> onGrasp = null) => pieceHolder.Grasp(piece, onGrasp);
    public void Grasp(IPieceHolder other, Action<Piece> onGrasp = null) => pieceHolder.Grasp(other, onGrasp);
    public void Grasp(List<Piece> otherPieces, int count = -1, Action<Piece> onGrasp = null) => pieceHolder.Grasp(otherPieces, count, onGrasp);

    #endregion


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
            int n = (int) Mathf.Sqrt(PieceHolder.MaxPiecesSupported);
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