using System;
using UnityEngine;

namespace Gameplay.MagicFlower
{
    public class MagicFlowerSpace : MonoBehaviour
    {
        [SerializeField, Min(.1f)] private float spacing = .1f;
        [SerializeField] private Vector2Int tableSize = Vector2Int.one;
        [SerializeField] private GameObject prefab;

        public void Spawn()
        {
            var x = UnityEngine.Random.Range(0, tableSize.x);
            var y = UnityEngine.Random.Range(0, tableSize.y);
            var pos = GetCell(x, y);
            var o = Instantiate(prefab, transform);
            o.transform.position = pos;
            o.transform.Rotate(Vector3.up, UnityEngine.Random.Range(-180f, 180f), Space.World);
        }

        private Vector3 GetCell(int x, int y)
        {
            var left = -tableSize.x / 2f * spacing;
            var bottom = -tableSize.y / 2f * spacing;
            return transform.TransformPoint(new Vector3(left + x * spacing, 0, bottom + y * spacing));
        }

        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            var left = -tableSize.x / 2f * spacing;
            var bottom = -tableSize.y / 2f * spacing;
            for (var i = 0; i < tableSize.x; i++)
            {
                for (var j = 0; j < tableSize.y; j++)
                {
                    Gizmos.DrawWireCube(Vector3.zero + new Vector3(left + i * spacing, 0, bottom + j * spacing),
                        Vector3.one * spacing);
                }
            }
        }
    }
}