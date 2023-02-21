using System;
using Gameplay.Board;
using Gameplay.GameInteract.Button;
using UnityEngine;

namespace Gameplay.GameInteract
{
    public class TileChooser : MonoBehaviour
    {
        [SerializeField] private ButtonGroup buttonGroup;
        [SerializeField] private ButtonOnGround tileChoosingButtonPrefab;

        private ButtonOnGround[] _buttons;
        private Board.Board _board;
        private PlayersManager _playerManager;
        [field: System.NonSerialized] public ITile SelectedTile { get; private set; }
        public event Action SelectedTileChangedEvent;

        public void Setup(Board.Board board, PlayersManager playerManager)
        {
            _board = board;
            _playerManager = playerManager;

            _buttons = new ButtonOnGround[_board.Metadata.NumCitizenTilesPerSide];

            for (var i = 0; i < _board.Metadata.NumCitizenTilesPerSide; i++)
            {
                _buttons[i] = Instantiate(tileChoosingButtonPrefab, transform);
                _buttons[i].ClickedEvent += OnButtonClicked;
            }

            buttonGroup.Setup(_buttons);
        }

        public void TearDown()
        {
            buttonGroup.TearDown();

            foreach (var bt in _buttons)
            {
                bt.ClickedEvent -= OnButtonClicked;
                Destroy(bt.gameObject);
            }

            _buttons = null;
            SelectedTile = null;
        }

        public void ResetAll()
        {
            buttonGroup.HideButtons();
            SelectedTile = null;
        }

        private void OnButtonClicked(IButton obj)
        {
            if (obj is not TileChoosingButton btn) return;

            SelectedTile?.Transform.GetComponent<TileSelectable>()?.Unselect();
            SelectedTile = _board.Sides[_playerManager.CurrentPlayer.Index].CitizenTiles[Array.IndexOf(_buttons, btn)];
            SelectedTile?.Transform.GetComponent<TileSelectable>()?.Select();

            SelectedTileChangedEvent?.Invoke();

            buttonGroup.HideButtons();
        }

        public void ShowUp()
        {
            UpdateSide();
            buttonGroup.ShowButtons();
        }

        private void UpdateSide()
        {
            var tiles = _board.Sides[_playerManager.CurrentPlayer.Index].CitizenTiles;
            for (var i = 0; i < tiles.Count; i++)
            {
                var target = tiles[i].Transform;
                var tilePos = target.position;
                var tileRot = target.rotation;
                _buttons[i].transform.SetPositionAndRotation(tilePos + target.forward * tiles[i].Size, tileRot);
            }
        }
    }
}