using System;
using System.Linq;
using Common.Misc;
using DG.Tweening;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gameplay.GameInteract.Button
{
    public interface IButtonDisplayInfo
    {
    }

    public abstract class AButtonDisplay : MonoBehaviour
    {
        public abstract void SetDisplayInfo(IButtonDisplayInfo displayInfo);
    }

    public interface IButton
    {
        void ShowUp();
        void HideAway();
        void SetPositionAndRotation(Vector3 pos, Quaternion rot);
        bool IsShowing { get; }
        event Action<IButton> ClickedEvent;
        event Action<IButton> ActiveChangedEvent;
    }

    public interface IButtonFactory
    {
        IButton Spawn();
        void Destroy(IButton btn);
    }

    public class ButtonFactory : IButtonFactory
    {
        private readonly ButtonOnGround _buttonPrefab;
        private readonly Transform _container;

        public ButtonFactory(ButtonOnGround buttonPrefab, Transform container)
        {
            _buttonPrefab = buttonPrefab;
            _container = container;
        }

        public IButton Spawn()
        {
            return Object.Instantiate(_buttonPrefab, _container);
        }

        public void Destroy(IButton button)
        {
            var buttonOnGround = _container.GetComponentsInChildren<ButtonOnGround>().FirstOrDefault(b => b == (ButtonOnGround) button);
            if (buttonOnGround != null)
            {
                Object.Destroy(buttonOnGround.gameObject);
            }
        }
    }

    public class ButtonOnGround : MonoBehaviour, IButton
    {
        [SerializeField] private ABoundsClicker visual;
        [SerializeField] private AButtonDisplay display;

        [field: NonSerialized] public bool IsShowing { get; private set; }

        public event Action<IButton> ClickedEvent;
        public event Action<IButton> ActiveChangedEvent;

        private float VisualHeight => visual.Bounds.size.y;
        public AButtonDisplay Display => display;

        private void Start()
        {
            IsShowing = false;
            visual.gameObject.SetActive(false);
            visual.Clicked += OnClicked;
        }

        private void OnDestroy()
        {
            visual.Clicked -= OnClicked;
        }

        public virtual void ShowUp()
        {
            StopAllCoroutines();

            visual.gameObject.SetActive(true);

            visual.transform.localPosition = -Vector3.up * (VisualHeight * 0.5f);
            visual.transform.DOLocalMoveY(VisualHeight * 0.5f, .15f).OnComplete(() =>
            {
                IsShowing = true;
                visual.SetInteractable(true);
                ActiveChangedEvent?.Invoke(this);
            }).SetLink(visual.gameObject);
        }

        public void HideAway()
        {
            HideAway(.15f);
        }

        private void HideAway(float duration)
        {
            if (!IsShowing) return;

            visual.SetInteractable(false);
            visual.transform.DOLocalMoveY(-VisualHeight * 0.5f, duration)
                .OnComplete(() =>
                {
                    visual.gameObject.SetActive(false);
                    IsShowing = false;
                    ActiveChangedEvent?.Invoke(this);
                }).SetLink(visual.gameObject);
        }

        [ContextMenu("Click")]
        public void OnClicked(EventArgs eventArgs)
        {
            if (!IsShowing) return;
            HideAway(.05f);
            ClickedEvent?.Invoke(this);
        }

        public void SetPositionAndRotation(Vector3 pos, Quaternion rot)
        {
            transform.SetPositionAndRotation(pos, rot);
        }
    }
}