using UnityEngine;

namespace Gameplay.GameInteract
{
    public class ActionChooser : MonoBehaviour
    {
        [SerializeField] private ButtonContainer buttonContainer;

        private ButtonData[] _commands;

        public void ShowUp(Vector3 position, Quaternion rotation, GameInteractManager interact)
        {
            transform.position = position;
            transform.rotation = rotation;

            if (_commands == null)
            {
                _commands = new ButtonData[6];
                // _commands[0] =  new  new MoveCommand(buttonContainer, interact, false);
                // _commands[1] = new MoveCommand(buttonContainer, interact, true);
                // _commands[2] = new CancelActionChooserCommand(buttonContainer, interact);
                //
                // _commands[3] = new SpecialMoveCommand(buttonContainer);
                // _commands[4] = new SpecialMoveCommand(buttonContainer);
                // _commands[5] = new SpecialMoveCommand(buttonContainer);
            }

            buttonContainer.Setup(_commands);

            foreach (var bv in buttonContainer.ButtonViews)
            {
                if (bv.Command != null)
                {
                    bv.Display.SetDisplayInfo(new ButtonDisplaySpecialActionData());
                }
            }

            buttonContainer.ShowButtons();
        }

        public class MoveCommand : ButtonContainer.ButtonCommand
        {
            private readonly bool _forward;
            private readonly GameInteractManager _interact;

            public MoveCommand(ButtonContainer container, GameInteractManager interact, bool forward) : base(container)
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
                _interact.NotifyTilesAdapters(_interact.ChosenTile, false);
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