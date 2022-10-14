using System;
using DG.Tweening;
using SNM;
using TMPro;
using UnityEngine;

namespace Gameplay.GameInteract
{
    public class OnGroundButton : MonoBehaviour
    {
        [SerializeField] private ABoundsClicker visual;

        [field: NonSerialized] public bool Active { get; private set; }
        [field: NonSerialized] public int ID { get; private set; }

        private ICommand _command;
        private float VisualHeight => visual.Bounds.size.y;

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

        public void SetupCallback(int id, ICommand command)
        {
            ID = id;
            _command = command;
        }

        public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            transform.position = position;
            transform.rotation = rotation;
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
            visual.transform.DOLocalMoveY(-VisualHeight * 0.5f, duration).OnComplete(() => { visual.gameObject.SetActive(Active); }).SetLink(visual.gameObject);
        }

        [ContextMenu("Click")]
        public void Click()
        {
            HideAway(.05f);
            this.Delay(.2f, () => { _command.Execute(); });
        }
    }
}