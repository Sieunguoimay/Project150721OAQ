using System;
using System.Linq;
using UnityEngine;

namespace Gameplay.GameInteract
{
    public class DirectionChooser : MonoBehaviour
    {
        [SerializeField] private ButtonChooser buttonChooser;
        private ButtonChooser.ButtonData[] _buttons;
        private Action<int> _onResult;

        private void Start()
        {
            _buttons = new ButtonChooser.ButtonData[6];
            _buttons[0] = new ButtonChooser.ButtonData
            {
                ID = 0
            };
            _buttons[1] = new ButtonChooser.ButtonData
            {
                ID = 1
            };
            _buttons[2] = new ButtonChooser.ButtonData
            {
                ID = 2
            };
            _buttons[3] = new ButtonChooser.ButtonData
            {
                ID = 3
            };
            _buttons[4] = new ButtonChooser.ButtonData
            {
                ID = 4
            };
            _buttons[5] = new ButtonChooser.ButtonData
            {
                ID = 5
            };
        }

        public void ChooseDirection(Vector3 position, Quaternion rotation, Action<int> onResult)
        {
            _onResult = onResult;
            transform.position = position;
            transform.rotation = rotation;
            buttonChooser.Setup(_buttons, OnTileChooserResult);
            buttonChooser.ShowButtons();
        }

        private void OnTileChooserResult(int id)
        {
            _onResult?.Invoke(_buttons[id].ID);
        }
    }
}