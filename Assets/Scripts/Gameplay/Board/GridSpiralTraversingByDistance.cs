using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameplay.Board
{
    public class GridSpiralTraversingByDistance : MonoBehaviour
    {
        [SerializeField, Min(0)] private int ringSize = 0;
        [SerializeField] private bool sort;
        [SerializeField] private bool clip;

        private List<Vector2Int> _cachedDisc;

        public Vector2Int GetPointAtIndex(int index)
        {
            return _cachedDisc[index];
        }

        private void OnValidate()
        {
            if (clip)
            {
                _cachedDisc = GetDiscEnumerable(ringSize).Where(cell => cell.x * cell.x + cell.y * cell.y <= ringSize * ringSize).ToList();
            }
            else
            {
                _cachedDisc = GetDiscEnumerable(ringSize).ToList();
            }

            if (sort)
            {
                _cachedDisc.Sort((a, b) =>
                {
                    var da = a.x * a.x + a.y * a.y;
                    var db = b.x * b.x + b.y * b.y;
                    return da == db ? 0 : da < db ? -1 : 1;
                });
            }
        }

        private void OnDrawGizmos()
        {
            if (_cachedDisc is not {Count: > 0}) return;
            var index = 0;
            foreach (var cell in _cachedDisc)
            {
                var v = index / (float) _cachedDisc.Count;
                Gizmos.color = new Color(v, v, v, 1);
                Gizmos.DrawCube(new Vector3(cell.x, index * .01f, cell.y), new Vector3(1, .1f, 1));
                index++;
            }
        }

        private static IEnumerable<Vector2Int> GetDiscEnumerable(int radius)
        {
            for (var i = 0; i <= radius; i++)
            {
                foreach (var cell in GetRingEnumerable(i))
                    yield return cell;
            }
        }

        private static IEnumerable<Vector2Int> GetRingEnumerable(int radius)
        {
            if (radius == 0)
            {
                yield return Vector2Int.zero;
            }
            else
            {
                var currentPoint = new Vector2Int(radius, -(radius - 1));
                yield return currentPoint;

                for (var y = currentPoint.y + 1; y <= radius; y++)
                {
                    currentPoint = new Vector2Int(currentPoint.x, y);
                    yield return currentPoint;
                }

                for (var x = currentPoint.x - 1; x >= -radius; x--)
                {
                    currentPoint = new Vector2Int(x, currentPoint.y);
                    yield return currentPoint;
                }

                for (var y = currentPoint.y - 1; y >= -radius; y--)
                {
                    currentPoint = new Vector2Int(currentPoint.x, y);
                    yield return currentPoint;
                }

                for (var x = currentPoint.x + 1; x <= radius; x++)
                {
                    currentPoint = new Vector2Int(x, currentPoint.y);
                    yield return currentPoint;
                }
            }
        }
    }
}