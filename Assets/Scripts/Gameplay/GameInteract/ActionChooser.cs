using Gameplay.GameInteract.Button;
using UnityEngine;

namespace Gameplay.GameInteract
{
    public class ActionChooser : MonoBehaviour
    {
        [SerializeField] private ButtonContainer buttonContainer;

        private readonly ButtonData[] _commands = new ButtonData[6];

        public ButtonContainer ButtonContainer => buttonContainer;

        public void SetMoveCommands(ButtonContainer.ButtonCommand left, ButtonContainer.ButtonCommand right)
        {
            _commands[0] = new ButtonData(left, null);
            _commands[1] = new ButtonData(right, null);
        }

        public void SetupOtherCommands()
        {
            _commands[2] = new ButtonData(null, null);
            _commands[3] = new ButtonData(new SpecialMoveCommand(), new ButtonDisplayInfoSpecialAction());
            _commands[4] = new ButtonData(new SpecialMoveCommand(), new ButtonDisplayInfoSpecialAction());
            _commands[5] = new ButtonData(new SpecialMoveCommand(), new ButtonDisplayInfoSpecialAction());
            foreach (var c in _commands) c.Command?.SetContainer(buttonContainer);
        }

        public void ShowUp()
        {
            buttonContainer.Setup(_commands);
            buttonContainer.ShowButtons();
        }

        private class CancelActionChooserCommand : ButtonContainer.ButtonCommand
        {
            private readonly GameInteractManager _interact;

            public CancelActionChooserCommand(GameInteractManager interact)
            {
                _interact = interact;
            }

            public override void Execute()
            {
                base.Execute();
                GameInteractManager.NotifyTilesAdapters(_interact.ChosenTile, false);
                _interact.ShowTileChooser();
            }
        }

        private class SpecialMoveCommand : ButtonContainer.ButtonCommand
        {
            public override void Execute()
            {
                base.Execute();
                Debug.Log("Here we execute a special move!!");
            }
        }
    }
}