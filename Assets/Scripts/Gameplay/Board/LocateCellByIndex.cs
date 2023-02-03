using UnityEngine;

namespace Gameplay.Board
{
    public class LocateCellByIndex : MonoBehaviour
    {
        private int RingSize(int r) => 4 * r;
        private int DiscSize(int r) => 1 + 2 * (r * r + r);

        private int LowerBoundRadiusFromDiscSize(int s)
        {
             return (s - 2) / 2;
        }
    }
}