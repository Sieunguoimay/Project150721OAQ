using System;
using UnityEngine;
using UnityEngine.Events;

namespace SNM
{
    [RequireComponent(typeof(MeshRenderer))]
    public class MeshBoundsClicker : MonoBehaviour, RayPointer.ITarget
    {
        [SerializeField] private UnityEvent onClick;
        public event Action OnClick = delegate { };

        public Bounds Bounds => GetComponent<MeshRenderer>().bounds;

        private void OnEnable()
        {
            Main.Instance?.RayPointer.Register(this);
            Debug.Log("MeshBoundsClicker Enable");
        }

        private void OnDisable()
        {
            Main.Instance?.RayPointer.Unregister(this);
            Debug.Log("MeshBoundsClicker Disable");
        }

        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Bounds.size);
        }

        public void OnHit(Ray ray, float distance)
        {
            OnClick?.Invoke();
            onClick?.Invoke();
        }
    }
}