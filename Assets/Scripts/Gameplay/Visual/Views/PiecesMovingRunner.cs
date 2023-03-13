using System;
using System.Collections.Generic;
using Common;
using Framework.Resolver;
using Gameplay.CoreGameplay.Interactors.Simulation;
using Gameplay.Helpers;
using Gameplay.Visual.Board;
using Gameplay.Visual.Piece;
using SNM;
using UnityEngine;

namespace Gameplay.Visual.Views
{
    public class PiecesMovingRunner
    {
        private IReadOnlyList<MovingStep> _movingSteps;
        private IReadOnlyList<StepActionItem> _movingStepItems;
        private readonly List<StepActionItem> _completedMovingStepItems = new();
        private int _iterator;
        private GridLocator _gridLocator;
        private IGameplayContainer _container;
        private BoardVisualView _boardVisualView;

        public event Action<PiecesMovingRunner> AllMovingStepsExecutedEvent;

        public void SetupDependencies(IResolver resolver)
        {
            _container = resolver.Resolve<IGameplayContainer>();
            _gridLocator = resolver.Resolve<GridLocator>();
            _boardVisualView = resolver.Resolve<BoardVisualView>();
        }

        public void RunTheMoves(IReadOnlyList<MovingStep> movingSteps)
        {
            _movingSteps = movingSteps;
            _iterator = 0;
            NextStep();
        }

        public void ResetMovingSteps()
        {
            _movingSteps = null;
            _completedMovingStepItems.Clear();
        }

        private void NextStep()
        {
            if (_movingSteps == null || _movingSteps.Count == 0) return;

            if (_iterator < _movingSteps.Count)
            {
                _completedMovingStepItems.Clear();
                _movingStepItems = _movingSteps[_iterator++].StepActionItems;
                foreach (var item in _movingStepItems)
                {
                    var executor = CreateStepExecutor(item);
                    executor.Execute();
                }
            }
            else
            {
                _movingSteps = null;
                AllMovingStepsExecutedEvent?.Invoke(this);
            }
        }

        private void OnStepExecutionDone(StepActionItem stepActionItem)
        {
            if (_completedMovingStepItems.Contains(stepActionItem))
            {
                Debug.LogError("MovingStepItem already executed");
                return;
            }

            _completedMovingStepItems.Add(stepActionItem);
            if (_completedMovingStepItems.Count == _movingStepItems.Count)
            {
                NextStep();
            }
        }

        private TileVisual GetTile(int index)
        {
            return _boardVisualView.BoardVisual.TileVisuals[index];
        }

        private IPieceContainer GetPieceBench()
        {
            return _container.PlayTurnTeller.CurrentTurn.PieceBench;
        }

        private MovingStepExecutor CreateStepExecutor(StepActionItem stepActionItem)
        {
            return stepActionItem.MoveType switch
            {
                MoveType.Grasp => new GraspExecutor(stepActionItem, this),
                MoveType.Drop => new DropExecutor(stepActionItem, this),
                MoveType.Slam => new SlamExecutor(stepActionItem, this),
                MoveType.Eat => new EatExecutor(stepActionItem, this),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private static Vector3 GetPosition(GridLocator gridLocator, IPieceContainer container, int index)
        {
            if (container is TileVisual tile)
            {
                var gridIndex = tile.GetNumTakenGridCells() + index;
                return gridLocator.GetPositionAtCellIndex(tile.transform, gridIndex);
            }

            if (container is Component c)
            {
                return c.transform.position;
            }

            return Vector3.zero;
        }

        private void MovePieces(IPieceContainer from, IPieceContainer to, int amount)
        {
            MovePieces(_gridLocator, from.HeldPieces, to, amount);
        }

        public static void MovePieces(GridLocator gridLocator, IReadOnlyList<Piece.Piece> citizens, IPieceContainer to, int amount)
        {
            var n = citizens.Count;
            for (var i = 0; i < amount; i++)
            {
                var index = n - i - 1;
                var target = GetPosition(gridLocator, to, i);
                if (citizens[index] is Citizen ci)
                {
                    ci.Animator.JumpTo(target, null);
                }
                else
                {
                    citizens[index].transform.position = target;
                }
            }
        }


        private class MovingStepExecutor
        {
            protected readonly StepActionItem StepActionItem;
            protected readonly PiecesMovingRunner Handler;

            protected MovingStepExecutor(StepActionItem stepActionItem, PiecesMovingRunner handler)
            {
                StepActionItem = stepActionItem;
                Handler = handler;
            }

            public virtual void Execute()
            {
                PublicExecutor.Instance.Delay(1, () => { Handler.OnStepExecutionDone(StepActionItem); });
            }
        }

        private class GraspExecutor : MovingStepExecutor
        {
            public GraspExecutor(StepActionItem stepActionItem, PiecesMovingRunner handler)
                : base(stepActionItem, handler)
            {
            }

            public override void Execute()
            {
                base.Execute();
            }
        }

        private class DropExecutor : MovingStepExecutor
        {
            public DropExecutor(StepActionItem stepActionItem, PiecesMovingRunner handler)
                : base(stepActionItem, handler)
            {
            }

            public override void Execute()
            {
                base.Execute();

                var from = Handler.GetTile(StepActionItem.PieceContainerIndex);
                var to = Handler.GetTile(StepActionItem.TargetPieceContainerIndex);
                var amount = from.HeldPieces.Count - StepActionItem.RemainingPieces;

                Handler.MovePieces(from, to, amount);
                IPieceContainer.TransferPiecesOwnerShip(from, to, amount);
            }
        }

        private class EatExecutor : MovingStepExecutor
        {
            public EatExecutor(StepActionItem stepActionItem, PiecesMovingRunner handler)
                : base(stepActionItem, handler)
            {
            }

            public override void Execute()
            {
                base.Execute();

                var from = Handler.GetTile(StepActionItem.TargetPieceContainerIndex);
                var to = Handler.GetPieceBench();
                var amount = from.HeldPieces.Count;

                Handler.MovePieces(from, to, amount);
                IPieceContainer.TransferAllPiecesOwnership(from, to);
            }
        }

        private class SlamExecutor : MovingStepExecutor
        {
            public SlamExecutor(StepActionItem stepActionItem, PiecesMovingRunner handler)
                : base(stepActionItem, handler)
            {
            }

            public override void Execute()
            {
                base.Execute();
            }
        }
    }
}