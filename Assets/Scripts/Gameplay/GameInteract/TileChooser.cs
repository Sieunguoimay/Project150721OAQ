using System;
using Gameplay.GameInteract.Button;
using Gameplay.Player;
using Gameplay.PlayTurn;
using Gameplay.Visual.Board;
using UnityEngine;

namespace Gameplay.GameInteract
{
    public interface ITileChooser
    {
        void Setup(IPlayTurnTeller turnTeller);
        void TearDown();
        void ResetAll();
        void ShowUp();
    }

    public class TileChooser : MonoBehaviour, ITileChooser
    {
        [SerializeField] private ButtonOnGround tileChoosingButtonPrefab;

        private IButton[] _buttons;

        private ButtonGroup _buttonGroup;
        private IPlayTurnTeller _turnTeller;
        [field: System.NonSerialized] public Tile SelectedTile { get; private set; }
        private Transform[] _cachedCitizenTileTransforms;
        public event Action SelectedTileChangedEvent;
        private IButtonFactory _buttonFactory;

        public void Setup(IPlayTurnTeller turnTeller)
        {
            _turnTeller = turnTeller;

            _cachedCitizenTileTransforms = _turnTeller.CurrentTurn.GetCitizenTilesTransform();
            var btnNum = _cachedCitizenTileTransforms.Length;
            _buttonFactory = new ButtonFactory(tileChoosingButtonPrefab, transform);
            _buttons = new IButton[btnNum];

            for (var i = 0; i < btnNum; i++)
            {
                _buttons[i] = _buttonFactory.Spawn();
                _buttons[i].ClickedEvent += OnButtonClicked;
            }

            _buttonGroup = new ButtonGroup(_buttons);
        }

        public void TearDown()
        {
            foreach (var bt in _buttons)
            {
                bt.ClickedEvent -= OnButtonClicked;
                _buttonFactory.Destroy(bt);
            }

            _buttons = null;
            SelectedTile = null;
        }

        public void ResetAll()
        {
            _buttonGroup.HideButtons();
            SelectedTile = null;
        }

        private void OnButtonClicked(IButton btn)
        {
            SelectedTile?.GetComponent<TileSelectable>()?.Unselect();
            var tr = _cachedCitizenTileTransforms[Array.IndexOf(_buttons, (ButtonOnGround) btn)];
            tr.GetComponent<TileSelectable>()?.Select();
            SelectedTile = tr.GetComponent<CitizenTile>();
            SelectedTileChangedEvent?.Invoke();

            _buttonGroup.HideButtons();
        }

        public void ShowUp()
        {
            UpdateButtonPositionOnCurrentSide();
            _buttonGroup.ShowButtons();
        }

        private void UpdateButtonPositionOnCurrentSide()
        {
            _cachedCitizenTileTransforms = _turnTeller.CurrentTurn.GetCitizenTilesTransform();
            for (var i = 0; i < _cachedCitizenTileTransforms.Length; i++)
            {
                var target = _cachedCitizenTileTransforms[i];
                var tileRot = target.rotation;
                var pos = CalculateButtonPosition(target, 1);
                _buttons[i].SetPositionAndRotation(pos, tileRot);
            }
        }

        private static Vector3 CalculateButtonPosition(Transform target, float offset)
        {
            return target.position + target.forward * offset;
        }
    }
}