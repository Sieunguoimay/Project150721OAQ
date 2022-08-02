﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameplay
{
    public interface IPieceHolder
    {
        List<Piece> Pieces { get; }
        void Grasp(Piece piece, Action<Piece> onGrasp = null);
        void Grasp(IPieceHolder other, Action<Piece> onGrasp = null);
        void Grasp(List<Piece> otherPieces, int count = -1, Action<Piece> onGrasp = null);
        void OnGrasp(IPieceHolder other);
    }

    public class PieceHolder : IPieceHolder
    {
        [field: System.NonSerialized] public List<Piece> Pieces { get; } = new();

        public virtual void Grasp(Piece piece, Action<Piece> onGrasp = null)
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

            other.OnGrasp(this);
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
                var n = Mathf.Min(count, otherPieces.Count);
                for (var i = n - 1; i >= 0; i--)
                {
                    Grasp(otherPieces[i], onGrasp);
                    otherPieces.RemoveAt(i);
                }
            }
        }

        public virtual void OnGrasp(IPieceHolder whom)
        {
        }
    }

    public class PieceContainer : MonoBehaviour, IPieceHolder
    {
        private const int MaxPiecesSupported = 50;
        private readonly Vector2Int[] _reservedPoints = new Vector2Int[MaxPiecesSupported];

        #region IPieceHolder

        private readonly PieceHolder _pieceHolder = new PieceDropper();
        public List<Piece> Pieces => _pieceHolder.Pieces;
        public virtual void Grasp(Piece piece, Action<Piece> onGrasp = null) => _pieceHolder.Grasp(piece, onGrasp);
        public void Grasp(IPieceHolder other, Action<Piece> onGrasp = null) => _pieceHolder.Grasp(other, onGrasp);

        public void Grasp(List<Piece> otherPieces, int count = -1, Action<Piece> onGrasp = null) =>
            _pieceHolder.Grasp(otherPieces, count, onGrasp);

        public virtual void OnGrasp(IPieceHolder whom)
        {
            _pieceHolder.OnGrasp(whom);
        }

        #endregion

        public virtual void Setup()
        {
            ReservePositionsInFilledCircle();
        }

        public void Reposition(Transform t)
        {
            t.position = GetPositionInFilledCircle(Mathf.Max(0, Pieces.Count - 1), false);
        }

        public virtual Vector3 GetPositionInFilledCircle(int index, bool local = false, float size = 0.15f)
        {
            var pos = new Vector3(_reservedPoints[index].x, 0, _reservedPoints[index].y) * size;
            if (!local)
            {
                pos = transform.TransformPoint(pos);
            }

            return pos;
        }

        private void ReservePositionsInFilledCircle()
        {
            var r = 1;
            var n = 0;
            var points = new List<Vector2Int>();
            while (n < MaxPiecesSupported)
            {
                n = 0;
                points.Clear();
                for (var x = -r; x <= r; x++)
                {
                    for (var y = -r; y <= r; y++)
                    {
                        if (x * x + y * y > r * r) continue;

                        points.Add(new Vector2Int(x, y));
                        n++;
                    }
                }

                r++;
            }

            points.Sort((a, b) =>
            {
                var da = a.x * a.x + a.y * a.y;
                var db = b.x * b.x + b.y * b.y;
                return (da == db ? 0 : (da < db ? -1 : 1));
            });

            for (var i = 0; i < MaxPiecesSupported; i++)
            {
                _reservedPoints[i] = points[i];
            }
        }

        public Vector3 SpawnPositionInCircle(int index, bool local = false, float size = 0.15f)
        {
            var points = new List<Vector2Int>();
            int r = 1;
            while (index > points.Count - 1)
            {
                points.AddRange(SNM.Math.BresenhamCircleAlgorithm(0, 0, r++));
            }

            var pos = new Vector3(points[index].x, 0, points[index].y) * size;
            if (!local)
            {
                pos = transform.TransformPoint(pos);
            }

            return pos;
        }

        public Vector3 SpawnPositionInUnityUnit(int index, bool local = false, float size = 0.15f)
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
                // int n = (int) Mathf.Sqrt(PieceHolder.MaxPiecesSupported);
                // float scale = 1f / n;
                pos.x = e.Item1 * size;
                pos.z = e.Item2 * size;
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
}