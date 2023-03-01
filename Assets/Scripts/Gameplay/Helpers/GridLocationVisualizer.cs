using UnityEngine;

namespace Gameplay.Helpers
{
    public class GridLocationVisualizer : MonoBehaviour
    {
        [SerializeField, Min(0)] private int numCells = 0;
        [SerializeField, Min(0)] private float size = 0.15f;

        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            for (var i = 0; i < numCells; i++)
            {
                var cell = GridLocationCalculate.GetPositionAtCellIndex(i);
                var v = i / (float) numCells;
                Gizmos.color = new Color(v, v, v, 1);

                Gizmos.DrawCube((new Vector3(cell.x, 0, cell.y) * size),
                    new Vector3(size, .1f, size));
            }
        }
    }
}