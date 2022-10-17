using Gameplay.GameInteract.Button;
using UnityEngine;

namespace Gameplay.GameInteract
{
    public class ActionChooser : MonoBehaviour
    {
        [SerializeField] private ButtonContainer buttonContainer;

        private ButtonData[] _commands;

        public void ShowUp(GameInteractManager interact)
        {

            if (_commands == null)
            {
                _commands = new ButtonData[6];
                _commands[0] = new ButtonData(new MoveButtonCommand(buttonContainer, interact, false), null);
                _commands[1] = new ButtonData(new MoveButtonCommand(buttonContainer, interact, true),null);
                _commands[2] = new ButtonData(new CancelActionChooserCommand(buttonContainer, interact),null);

                _commands[3] = new ButtonData(new SpecialMoveCommand(buttonContainer),new ButtonDisplayInfoSpecialAction());
                _commands[4] = new ButtonData(new SpecialMoveCommand(buttonContainer),new ButtonDisplayInfoSpecialAction());
                _commands[5] = new ButtonData(new SpecialMoveCommand(buttonContainer),new ButtonDisplayInfoSpecialAction());
            }                                  

            buttonContainer.Setup(_commands);
            buttonContainer.ShowButtons();
        }

        public class MoveButtonCommand : ButtonContainer.ButtonCommand
        {
            private readonly bool _forward;
            private readonly GameInteractManager _interact;

            public MoveButtonCommand(ButtonContainer container, GameInteractManager interact, bool forward) : base(container)
            {
                _forward = forward;
                _interact = interact;
            }

            public override void Execute()
            {
                base.Execute();
                _interact.MoveLeftRight(_forward);
            }
        }

        public class CancelActionChooserCommand : ButtonContainer.ButtonCommand
        {
            private readonly GameInteractManager _interact;

            public CancelActionChooserCommand(ButtonContainer container, GameInteractManager interact) : base(container)
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

        public class SpecialMoveCommand : ButtonContainer.ButtonCommand
        {
            public SpecialMoveCommand(ButtonContainer container) : base(container)
            {
            }

            public override void Execute()
            {
                base.Execute();
                Debug.Log("Here we execute a special move!!");
            }
        }
    }
}