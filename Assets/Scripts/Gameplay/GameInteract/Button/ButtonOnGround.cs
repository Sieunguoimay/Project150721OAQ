using System;
using Common.Misc;
using DG.Tweening;
using SNM;
using UnityEngine;

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
        bool IsShowing { get; }
        event Action<IButton> ClickedEvent;
        event Action<IButton> ActiveChangedEvent;
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
            if (IsShowing) return;
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
            if (!IsShowing) return;
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
    }
}