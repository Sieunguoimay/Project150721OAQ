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

        public void Cleanup()
        {
            _movingSteps = null;
        }

        private void NextStep()
        {
            if (_movingSteps == null) return;

            if (_iterator < _movingSteps.Count)
            {
                var step = _movingSteps[_iterator++];
                var executor = CreateStepExecutor(step);
                executor.Execute();
            }
            else
            {
                _movingSteps = null;
                AllMovingStepsExecutedEvent?.Invoke(this);
            }
        }

        private void OnStepExecutionDone()
        {
            NextStep();
        }

        private Tile GetTile(int index)
        {
            return _boardVisualView.BoardVisual.Tiles[index];
        }

        private IPieceContainer GetPieceBench()
        {
            return _container.PlayTurnTeller.CurrentTurn.PieceBench;
        }

        private MovingStepExecutor CreateStepExecutor(MovingStep step)
        {
            return step.MoveType switch
            {
                MoveType.Grasp => new GraspExecutor(step, this),
                MoveType.Drop => new DropExecutor(step, this),
                MoveType.Slam => new SlamExecutor(step, this),
                MoveType.Eat => new EatExecutor(step, this),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private Vector3 GetPosition(IPieceContainer container, int index)
        {
            if (container is Tile tile)
            {
                var gridIndex = tile.GetNumTakenGridCells() + index;
                return _gridLocator.GetPositionAtCellIndex(tile.transform, gridIndex);
            }

            if (container is Component c)
            {
                return c.transform.position;
            }

            return Vector3.zero;
        }

        private void MovePieces(IPieceContainer from, IPieceContainer to, int amount)
        {
            var citizens = from.HeldPieces;
            var n = citizens.Count;
            for (var i = 0; i < amount; i++)
            {
                var index = n - i - 1;
                var target = GetPosition(to, i);
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
            protected readonly MovingStep MovingStep;
            protected readonly PiecesMovingRunner Handler;

            protected MovingStepExecutor(MovingStep movingStep, PiecesMovingRunner handler)
            {
                MovingStep = movingStep;
                Handler = handler;
            }

            public virtual void Execute()
            {
                PublicExecutor.Instance.Delay(1, () => { Handler.OnStepExecutionDone(); });
            }
        }

        private class GraspExecutor : MovingStepExecutor
        {
            public GraspExecutor(MovingStep movingStep, PiecesMovingRunner handler)
                : base(movingStep, handler)
            {
            }

            public override void Execute()
            {
                base.Execute();
            }
        }

        private class DropExecutor : MovingStepExecutor
        {
            public DropExecutor(MovingStep movingStep, PiecesMovingRunner handler)
                : base(movingStep, handler)
            {
            }

            public override void Execute()
            {
                base.Execute();

                var from = Handler.GetTile(MovingStep.PieceContainerIndex);
                var to = Handler.GetTile(MovingStep.TargetPieceContainerIndex);
                var amount = from.HeldPieces.Count - MovingStep.RemainingPieces;

                Handler.MovePieces(from, to, amount);
                IPieceContainer.TransferPiecesOwnerShip(from, to, amount);
            }
        }

        private class EatExecutor : MovingStepExecutor
        {
            public EatExecutor(MovingStep movingStep, PiecesMovingRunner handler)
                : base(movingStep, handler)
            {
            }

            public override void Execute()
            {
                base.Execute();

                var from = Handler.GetTile(MovingStep.TargetPieceContainerIndex);
                var to = Handler.GetPieceBench();
                var amount = from.HeldPieces.Count;

                Handler.MovePieces(from, to, amount);
                IPieceContainer.TransferAllPiecesOwnership(from, to);
            }
        }

        private class SlamExecutor : MovingStepExecutor
        {
            public SlamExecutor(MovingStep movingStep, PiecesMovingRunner handler)
                : base(movingStep, handler)
            {
            }

            public override void Execute()
            {
                base.Execute();
            }
        }
    }
}