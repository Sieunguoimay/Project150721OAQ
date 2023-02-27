using UnityEngine;

namespace Gameplay.Piece
{
    public class Piece : MonoBehaviour
    {
        [SerializeField] private Vector3 size;
        public Transform Transform => transform;

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            var t = transform;
            Gizmos.DrawWireCube(t.position + t.up * size.y * 0.5f, size);
        }
#endif
    }
}