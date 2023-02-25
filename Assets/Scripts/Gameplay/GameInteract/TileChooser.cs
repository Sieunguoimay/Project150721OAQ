using System;
using Gameplay.Board;
using Gameplay.GameInteract.Button;
using Gameplay.Player;
using UnityEngine;

namespace Gameplay.GameInteract
{
    public interface ITileChooser
    {
        void Setup(Board.Board board, PlayerController playerManager);
        void TearDown();
        void ResetAll();
        void ShowUp();
    }

    public class TileChooser : MonoBehaviour, ITileChooser
    {
        [SerializeField] private ButtonOnGround tileChoosingButtonPrefab;

        private IButton[] _buttons;

        private Board.Board _board;
        private PlayerController _playerManager;
        private ButtonGroup _buttonGroup;

        [field: System.NonSerialized] public ITile SelectedTile { get; private set; }
        public event Action SelectedTileChangedEvent;
        private IButtonFactory _buttonFactory;

        public void Setup(Board.Board board, PlayerController playerManager)
        {
            _board = board;
            _playerManager = playerManager;
            _buttonFactory = new ButtonFactory(tileChoosingButtonPrefab, transform);
            _buttons = new IButton[_board.Metadata.NumCitizenTilesPerSide];

            for (var i = 0; i < _board.Metadata.NumCitizenTilesPerSide; i++)
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
            SelectedTile?.Transform.GetComponent<TileSelectable>()?.Unselect();
            SelectedTile = _board.Sides[_playerManager.CurrentPlayer.Index].CitizenTiles[Array.IndexOf(_buttons, (ButtonOnGround) btn)];
            SelectedTile?.Transform.GetComponent<TileSelectable>()?.Select();

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
            var tilesOnSide = _board.Sides[_playerManager.CurrentPlayer.Index].CitizenTiles;
            for (var i = 0; i < tilesOnSide.Count; i++)
            {
                var target = tilesOnSide[i].Transform;
                var tileRot = target.rotation;
                var pos = CalculateButtonPosition(target, tilesOnSide[i].Size);
                _buttons[i].SetPositionAndRotation(pos, tileRot);
            }
        }

        private static Vector3 CalculateButtonPosition(Transform target, float offset)
        {
            return target.position + target.forward * offset;
        }
    }
}