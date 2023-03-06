using System;
using System.Collections.Generic;
using Framework.Resolver;
using Gameplay.BambooStick;
using Gameplay.CoreGameplay.Interactors;
using Gameplay.CoreGameplay.Interactors.Simulation;
using Gameplay.Helpers;
using Gameplay.Visual.Board;
using Gameplay.Visual.Piece;
using UnityEngine;

namespace Gameplay.Visual
{
    [CreateAssetMenu]
    public class CoreGameplayVisualPresenter :
        BaseGenericDependencyInversionScriptableObject<CoreGameplayVisualPresenter>,
        IRefreshResultHandler
    {
        private IGameplayContainer _container;
        private BambooFamilyManager _bambooFamily;
        private BoardCreator _boardCreator;
        private PieceGenerator _pieceGenerator;
        private GridLocator _gridLocator;
        public PiecesMovingRunner MovingRunner { get; private set; }
        private SimulationResultPresenter _simulationResultPresenter;

        public event Action<CoreGameplayVisualPresenter> VisualReadyEvent;
        public Board.Board BoardVisual { get; private set; }

        protected override void OnBind(IBinder binder)
        {
            base.OnBind(binder);
            _simulationResultPresenter = new SimulationResultPresenter(this);
            binder.Bind<BoardStatePresenter>(new BoardStatePresenter());
            binder.Bind<IBoardMoveSimulationResultHandler>(_simulationResultPresenter);
        }

        protected override void OnUnbind(IBinder binder)
        {
            base.OnUnbind(binder);
            binder.Unbind<BoardStatePresenter>();
            binder.Unbind<IBoardMoveSimulationResultHandler>();
        }

        protected override void OnSetupDependencies()
        {
            base.OnSetupDependencies();

            _container = Resolver.Resolve<IGameplayContainer>();
            _bambooFamily = Resolver.Resolve<BambooFamilyManager>();
            _boardCreator = Resolver.Resolve<BoardCreator>();
            _pieceGenerator = Resolver.Resolve<PieceGenerator>();
            _gridLocator = Resolver.Resolve<GridLocator>();
            MovingRunner = new PiecesMovingRunner(_gridLocator, Resolver.Resolve<IGameplayContainer>());

            _simulationResultPresenter.MoveStepsAvailableEvent -= OnMoveStepsAvailable;
            _simulationResultPresenter.MoveStepsAvailableEvent += OnMoveStepsAvailable;
        }

        protected override void OnTearDownDependencies()
        {
            base.OnTearDownDependencies();
            _simulationResultPresenter.MoveStepsAvailableEvent -= OnMoveStepsAvailable;
        }

        private void OnMoveStepsAvailable(SimulationResultPresenter arg1, IReadOnlyList<MovingStep> movingSteps)
        {
            MovingRunner.RunTheMoves(movingSteps);
        }

        public void HandleRefreshData(RefreshData refreshData)
        {
            GenerateBoardVisual();
        }

        private void GenerateBoardVisual()
        {
            var matchData = _container.MatchData;
            var numSides = matchData.playerNum;
            var tilesPerSide = matchData.tilesPerGroup;
            var piecesPerTile = matchData.numCitizensInTile;

            BoardVisual = _boardCreator.CreateBoard(numSides, tilesPerSide);

            _pieceGenerator.SpawnPieces(numSides, tilesPerSide, piecesPerTile);

            new PieceRelease(_pieceGenerator.Citizens, _pieceGenerator.Mandarins, piecesPerTile,
                BoardVisual, _gridLocator, OnAllPiecesInPlace).ReleasePieces();

            _bambooFamily.BeginAnimSequence();
        }

        public void Cleanup()
        {
            _bambooFamily.ResetAll();
            _pieceGenerator.DeletePieces();
            BoardCreator.DeleteBoard(BoardVisual);
        }

        private void OnAllPiecesInPlace()
        {
            VisualReadyEvent?.Invoke(this);
        }
    }
}