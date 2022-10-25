using System;
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

    public class ButtonData
    {
        public ButtonData(ButtonContainer.ButtonCommand command, IButtonDisplayInfo displayInfo)
        {
            Command = command;
            DisplayInfo = displayInfo;
        }

        public ButtonContainer.ButtonCommand Command { get; }
        public IButtonDisplayInfo DisplayInfo { get; }
    }

    public class OnGroundButton : MonoBehaviour
    {
        [SerializeField] private ABoundsClicker visual;
        [SerializeField] private AButtonDisplay display;
        [field: NonSerialized] public bool Active { get; private set; }

        private float VisualHeight => visual.Bounds.size.y;
        public AButtonDisplay Display => display;

        [field: System.NonSerialized] public ICommand Command { get; private set; }

        private void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            Active = false;
            visual.gameObject.SetActive(Active);
            visual.Clicked.AddListener(Click);
        }

        public void SetCommand(ICommand command)
        {
            Command = command;
        }

        public void ShowUp()
        {
            visual.gameObject.SetActive(true);

            visual.transform.localPosition = -Vector3.up * (VisualHeight * 0.5f);
            visual.transform.DOLocalMoveY(VisualHeight * 0.5f, .15f).OnComplete(() =>
            {
                Active = true;
                visual.SetInteractable(true);
            }).SetLink(visual.gameObject);
        }

        public void HideAway()
        {
            HideAway(.15f);
        }

        private void HideAway(float duration)
        {
            if (!Active) return;
            Active = false;
            visual.SetInteractable(false);
            visual.transform.DOLocalMoveY(-VisualHeight * 0.5f, duration)
                .OnComplete(() => { visual.gameObject.SetActive(Active); }).SetLink(visual.gameObject);
        }

        [ContextMenu("Click")]
        public void Click()
        {
            HideAway(.05f);
            this.Delay(.2f, () => { Command?.Execute(); });
        }
    }
}