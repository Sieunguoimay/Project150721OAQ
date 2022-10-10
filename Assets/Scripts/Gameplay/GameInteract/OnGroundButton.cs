using System;
using DG.Tweening;
using SNM;
using UnityEngine;

namespace Gameplay.GameInteract
{
    public class OnGroundButton : MonoBehaviour, RayPointer.IRaycastTarget
    {
        [SerializeField] private Transform visual;
        [SerializeField] private Vector3 size;

        [field: NonSerialized] public bool Active { get; private set; }
        [field: NonSerialized] public object AttachedData { get; private set; }
        private Action<OnGroundButton> _onClick;

        private void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            Active = false;
            visual.gameObject.SetActive(Active);
        }

        public void RiseUp(Vector3 position, Quaternion rotation, object data, Action<OnGroundButton> onClick)
        {
            transform.position = position;
            transform.rotation = rotation;
            AttachedData = data;
            _onClick = onClick;
            visual.transform.localPosition = -Vector3.up * (size.y * 0.5f);
            visual.gameObject.SetActive(true);
            visual.transform.DOLocalMoveY(0, .15f).OnComplete(() =>
            {
                Active = true;
                RayPointer.Instance.Register(this);
            }).SetLink(visual.gameObject);
        }

        public void HideAway()
        {
            if (!Active) return;
            Active = false;
            RayPointer.Instance.Unregister(this);
            visual.transform.DOLocalMoveY(-size.y * 0.5f, .15f).OnComplete(() =>
            {
                visual.gameObject.SetActive(Active);
            }).SetLink(visual.gameObject);
        }

        public void Click()
        {
            HideAway();
            this.Delay(.2f, () =>
            {
                _onClick?.Invoke(this);
            });
        }

        public Bounds Bounds => new(transform.position, size);

        public void OnHit(Ray ray, float distance)
        {
            Click();
        }
    }
}