using System;
using SNM;
using UnityEngine;

namespace Gameplay.GameInteract
{
    public class OnGroundButton : MonoBehaviour, RayPointer.IRaycastTarget
    {
        [SerializeField] private Transform visual;
        [SerializeField] private Vector2 size;

        [field: NonSerialized] public bool Active { get; private set; }
        [field: NonSerialized] public object AttachedData { get; private set; }
        private  Action<OnGroundButton> _onClick;
        
        private void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            Active = false;
            visual.gameObject.SetActive(Active);
        }

        public void RiseUp(Vector3 position, object data, Action<OnGroundButton> onClick)
        {
            transform.position = position;
            AttachedData = data;
            _onClick = onClick;
            Active = true;
            RayPointer.Instance.Register(this);
            visual.gameObject.SetActive(Active);
        }

        public void HideAway()
        {
            Active = false;
            RayPointer.Instance.Unregister(this);
            visual.gameObject.SetActive(Active);
        }

        public void Click()
        {
            _onClick?.Invoke(this);
        }

        public Bounds Bounds => new(transform.position, new Vector3(size.x, .1f, size.y));

        public void OnHit(Ray ray, float distance)
        {
            Click();
        }
    }
}