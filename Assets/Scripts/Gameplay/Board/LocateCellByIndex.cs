using SNM;
using UnityEngine;

namespace Gameplay.Board
{
    public class LocateCellByIndex : MonoBehaviour
    {
        private int NumCellsOnRing(int r) => 4 * r;
        private int NumCellsInDisc(int r) => 1 + 2 * (r * r + r);

        private int LowerBoundRadiusFromNumCells(int s) => Mathf.FloorToInt(RadiusFromNumCells(s));
        private int UpperBoundRadiusFromNumCells(int s) => Mathf.CeilToInt(RadiusFromNumCells(s));

        private static float RadiusFromNumCells(int s)
        {
            return (Mathf.Sqrt(2 * s - 1) - 1) / 2;
        }

        // public Vector2Int GetCellAtIndex(int index)
        // {
        //      
        // }
        [ContextMenu("Test")]
          private void Test()
        {
            for (var i = 0; i < 20; i++)
            {
                Debug.Log(RadiusFromNumCells(i + 1));
            }
        }
    }
}