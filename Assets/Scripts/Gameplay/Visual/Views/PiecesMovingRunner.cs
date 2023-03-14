using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Framework.Resolver;
using Gameplay.CoreGameplay.Controllers;
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
                Debug.Log($"Execute step items: {string.Join(" ", _movingStepItems.Select(i => $"{i.MoveType} - {i.TargetPieceContainerIndex}"))}");
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
                Debug.Log($"OnStepExecutionDone: {string.Join(" ", _completedMovingStepItems.Select(i => i.MoveType))}");
                PublicExecutor.Instance.ExecuteInNextFrame(NextStep);
                // NextStep();
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
                // if (citizens[index] is Citizen ci)
                // {
                //     ci.Animator.JumpTo(target, null);
                // }
                // else
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

            public void Execute()
            {
                OnExecuteBegin();
                PublicExecutor.Instance.Delay(1, () =>
                {
                    OnExecuteEnd();
                    Handler.OnStepExecutionDone(StepActionItem);
                });
            }

            protected virtual void OnExecuteBegin()
            {
            }

            protected virtual void OnExecuteEnd()
            {
            }
        }

        private class GraspExecutor : MovingStepExecutor
        {
            public GraspExecutor(StepActionItem stepActionItem, PiecesMovingRunner handler)
                : base(stepActionItem, handler)
            {
            }
        }

        private class DropExecutor : MovingStepExecutor
        {
            private TileVisual _to;
            private int _amount;
            private readonly SimplePieceContainer _container = new();

            public DropExecutor(StepActionItem stepActionItem, PiecesMovingRunner handler)
                : base(stepActionItem, handler)
            {
            }

            protected override void OnExecuteBegin()
            {
                base.OnExecuteBegin();

                var from = Handler.GetTile(StepActionItem.PieceContainerIndex);
                _to = Handler.GetTile(StepActionItem.TargetPieceContainerIndex);
                _amount = from.HeldPieces.Count - StepActionItem.RemainingPieces;

                Handler.MovePieces(from, _to, _amount);
                IPieceContainer.TransferPiecesOwnerShip(from, _to, _amount);
            }

            protected override void OnExecuteEnd()
            {
                base.OnExecuteEnd();

                // IPieceContainer.TransferPiecesOwnerShip(_container, _to, _amount);
            }
        }

        private class EatExecutor : MovingStepExecutor
        {
            private IPieceContainer _to;
            private int _amount;
            private readonly SimplePieceContainer _container = new();

            public EatExecutor(StepActionItem stepActionItem, PiecesMovingRunner handler)
                : base(stepActionItem, handler)
            {
            }

            protected override void OnExecuteBegin()
            {
                base.OnExecuteBegin();

                var from = Handler.GetTile(StepActionItem.TargetPieceContainerIndex);
                _to = Handler.GetPieceBench();
                _amount = from.HeldPieces.Count;

                Handler.MovePieces(from, _to, _amount);
                IPieceContainer.TransferAllPiecesOwnership(from, _to);
            }

            protected override void OnExecuteEnd()
            {
                base.OnExecuteEnd();
                // IPieceContainer.TransferAllPiecesOwnership(_container, _to);
            }
        }

        private class SlamExecutor : MovingStepExecutor
        {
            public SlamExecutor(StepActionItem stepActionItem, PiecesMovingRunner handler)
                : base(stepActionItem, handler)
            {
            }
        }
    }
}