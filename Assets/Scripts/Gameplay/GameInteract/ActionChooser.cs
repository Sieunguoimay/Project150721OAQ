using UnityEngine;

namespace Gameplay.GameInteract
{
    public class ActionChooser : MonoBehaviour
    {
        [SerializeField] private ButtonChooser buttonChooser;

        private ICommand[] _commands;

        public void ShowUp(Vector3 position, Quaternion rotation, GameInteractManager interact)
        {
            transform.position = position;
            transform.rotation = rotation;

            if (_commands == null)
            {
                _commands = new ICommand[6];
                _commands[0] = new MoveCommand(buttonChooser, interact, false);
                _commands[1] = new MoveCommand(buttonChooser, interact, true);
                _commands[2] = new CancelActionChooserCommand(buttonChooser, interact);

                _commands[3] = new SpecialMoveCommand(buttonChooser);
                _commands[4] = new SpecialMoveCommand(buttonChooser);
                _commands[5] = new SpecialMoveCommand(buttonChooser);
            }

            buttonChooser.Setup(6, _commands);
            buttonChooser.ShowButtons();
        }

        public class MoveCommand : ButtonChooser.ButtonCommand
        {
            private readonly bool _forward;
            private readonly GameInteractManager _interact;

            public MoveCommand(ButtonChooser chooser, GameInteractManager interact, bool forward) : base(chooser)
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

        public class CancelActionChooserCommand : ButtonChooser.ButtonCommand
        {
            private readonly GameInteractManager _interact;

            public CancelActionChooserCommand(ButtonChooser chooser, GameInteractManager interact) : base(chooser)
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

        public class SpecialMoveCommand : ButtonChooser.ButtonCommand
        {
            public SpecialMoveCommand(ButtonChooser chooser) : base(chooser)
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