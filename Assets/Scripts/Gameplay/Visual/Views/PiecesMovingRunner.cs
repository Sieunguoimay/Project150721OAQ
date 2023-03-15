using System;
using System.Collections.Generic;
using System.Linq;
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
    public abstract class PiecesMovingRunner
    {
        protected int StepIterator;

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

        public abstract void ResetMovingSteps();
        protected abstract void NextStep();
        protected abstract IPieceContainer GetTempPieceContainer();

        protected abstract void OnStepExecutionDone();

        protected void OnAllMovingStepsExecutedEvent()
        {
            AllMovingStepsExecutedEvent?.Invoke(this);
        }

        private TileVisual GetTile(int index)
        {
            return _boardVisualView.BoardVisual.TileVisuals[index];
        }

        private IPieceContainer GetPieceBench()
        {
            return _container.PlayTurnTeller.CurrentTurn.PieceBench;
        }

        protected MovingStepExecutor CreateStepExecutor(MoveType moveType, int targetPieceContainerIndex)
        {
            return moveType switch
            {
                MoveType.Grasp => new GraspExecutor(targetPieceContainerIndex, this),
                MoveType.Drop => new DropExecutor(targetPieceContainerIndex, this),
                MoveType.Slam => new SlamExecutor(targetPieceContainerIndex, this),
                MoveType.Eat => new EatExecutor(targetPieceContainerIndex, this),
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

        private void MovePieces(IReadOnlyList<Piece.Piece> citizens, IPieceContainer to, int amount)
        {
            MovePieces(_gridLocator, citizens, to, amount);
        }

        private void MoveAllPieces(IReadOnlyList<Piece.Piece> citizens, IPieceContainer to)
        {
            MovePieces(_gridLocator, citizens, to, citizens.Count);
        }

        public static void MovePieces(GridLocator gridLocator, IReadOnlyList<Piece.Piece> citizens,
            IPieceContainer to, int amount)
        {
            var n = citizens.Count;
            for (var i = 0; i < amount; i++)
            {
                var index = n - i - 1;
                MoveSinglePiece(citizens[index], gridLocator, to, i);
            }
        }

        private void MoveSinglePiece(Component piece, IPieceContainer to, int i = 0)
        {
            MoveSinglePiece(piece, _gridLocator, to, i);
        }

        private static void MoveSinglePiece(Component piece, GridLocator gridLocator, IPieceContainer to, int i = 0)
        {
            var target = GetPosition(gridLocator, to, i);
            if (piece is Citizen ci)
            {
                ci.Animator.JumpTo(target, null);
            }
            else
            {
                if (piece == null)
                {
                    Debug.Log("Err");
                }
                piece.transform.position = target;
            }
        }

        protected class MovingStepExecutor
        {
            protected readonly int TargetPieceContainerIndex;
            protected readonly PiecesMovingRunner Handler;

            protected MovingStepExecutor(int targetPieceContainerIndex, PiecesMovingRunner handler)
            {
                TargetPieceContainerIndex = targetPieceContainerIndex;
                Handler = handler;
            }

            public virtual void Execute()
            {
                PublicExecutor.Instance.Delay(1, () => { Handler.OnStepExecutionDone(); });
            }
        }

        protected class GraspExecutor : MovingStepExecutor
        {
            public GraspExecutor(int targetPieceContainerIndex, PiecesMovingRunner handler)
                : base(targetPieceContainerIndex, handler)
            {
            }

            public override void Execute()
            {
                base.Execute();
                var targetTile = Handler.GetTile(TargetPieceContainerIndex);
                IPieceContainer.TransferAllPiecesOwnership(targetTile, Handler.GetTempPieceContainer());
            }
        }

        protected class DropExecutor : MovingStepExecutor
        {
            public DropExecutor(int targetPieceContainerIndex, PiecesMovingRunner handler)
                : base(targetPieceContainerIndex, handler)
            {
            }

            public override void Execute()
            {
                base.Execute();

                var container = Handler.GetTempPieceContainer();
                var to = Handler.GetTile(TargetPieceContainerIndex);

                var piece = container.HeldPieces.LastOrDefault();

                Handler.MoveSinglePiece(piece, to);
                
                to.AddPiece(piece);
                container.RemovePiece(piece);

                Handler.MoveAllPieces(Handler.GetTempPieceContainer().HeldPieces, to);
            }
        }

        protected class EatExecutor : MovingStepExecutor
        {
            public EatExecutor(int targetPieceContainerIndex, PiecesMovingRunner handler)
                : base(targetPieceContainerIndex, handler)
            {
            }

            public override void Execute()
            {
                base.Execute();

                var from = Handler.GetTile(TargetPieceContainerIndex);
                var to = Handler.GetPieceBench();
                var amount = from.HeldPieces.Count;

                Handler.MovePieces(from, to, amount);
                IPieceContainer.TransferAllPiecesOwnership(from, to);
            }
        }

        protected class SlamExecutor : MovingStepExecutor
        {
            public SlamExecutor(int targetPieceContainerIndex, PiecesMovingRunner handler)
                : base(targetPieceContainerIndex, handler)
            {
            }

            public override void Execute()
            {
                base.Execute();
            }
        }
    }
}