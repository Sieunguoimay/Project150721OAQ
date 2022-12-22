using System;
using UnityEngine;
using UnityEngine.Events;

namespace SNM
{
    public abstract class ABoundsClicker : MonoBehaviour, RayPointer.IRaycastTarget
    {
        [SerializeField] private UnityEvent click;
        [SerializeField] private bool selfSetup = true;

        // [field: NonSerialized] public UnityEvent<ABoundsClicker> Clicked { get; private set; } = new();
        public event Action<EventArgs> Clicked;
        public abstract Bounds Bounds { get; }

        public class ClickedEventArgs : EventArgs
        {
            public MonoBehaviour Sender;
        }

        private ClickedEventArgs _clickedEventArgs;

        private void OnEnable()
        {
            InnerSetup();
        }

        private void OnDisable()
        {
            if (selfSetup)
            {
                SetInteractable(false);
            }
        }

        protected virtual void InnerSetup()
        {
            _clickedEventArgs = new ClickedEventArgs {Sender = this};
            if (selfSetup)
            {
                SetInteractable(true);
            }
        }

        public void SetInteractable(bool interactable)
        {
            if (gameObject.scene.buildIndex == 0) return;

            if (interactable)
            {
                RayPointer.Instance.Register(this);
            }
            else
            {
                RayPointer.Instance.Unregister(this);
            }
        }

        private void OnDrawGizmos()
        {
            // Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Bounds.center, Bounds.size);
        }

        public void OnHit(Ray ray, float distance)
        {
            click?.Invoke();
            Clicked?.Invoke(_clickedEventArgs);
        }
    }

    public class BoundsClicker : ABoundsClicker
    {
        [SerializeField] private Vector3 size;
        public override Bounds Bounds => new(transform.position, size);
    }
}