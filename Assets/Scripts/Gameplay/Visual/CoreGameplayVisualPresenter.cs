using System;
using Gameplay.BambooStick;
using Gameplay.CoreGameplay.Controllers;
using Gameplay.CoreGameplay.Interactors;
using Gameplay.CoreGameplay.Interactors.Simulation;
using Gameplay.CoreGameplay.Presenters;
using Gameplay.Helpers;
using Gameplay.Visual.Board;
using Gameplay.Visual.Piece;

namespace Gameplay.Visual
{
    public class CoreGameplayVisualPresenter :
        BaseGenericDependencyInversionScriptableObject<CoreGameplayVisualPresenter>, IInteractResultPresenter
    {
        private CoreGameplayContainer _coreGameplayContainer;
        private GameplayContainer _container;

        private BambooFamilyManager _bambooFamily;
        private BoardCreator _boardCreator;
        private PieceGenerator _pieceGenerator;
        private GridLocator _gridLocator;
        
        public event Action<CoreGameplayVisualPresenter> VisualReadyEvent;

        protected override void OnSetupDependencies()
        {
            base.OnSetupDependencies();
            _coreGameplayContainer = Resolver.Resolve<CoreGameplayContainer>();
            _container = Resolver.Resolve<GameplayContainer>();
            
            _bambooFamily = Resolver.Resolve<BambooFamilyManager>();
            _boardCreator = Resolver.Resolve<BoardCreator>();
            _pieceGenerator = Resolver.Resolve<PieceGenerator>();
            _gridLocator = Resolver.Resolve<GridLocator>();
        }

        public void Initialize()
        {
            _coreGameplayContainer.RefreshRequester.Refresh();
        }

        public void Cleanup()
        {
            _bambooFamily.ResetAll();
            _pieceGenerator.DeletePieces();
        }
        
        public void HandleRefreshData(RefreshData refreshData)
        {
            var matchData = _container.MatchData;

            var numSides = matchData.playerNum;
            var tilesPerSide = matchData.tilesPerGroup;
            var piecesPerTile = matchData.numCitizensInTile;
            
            _container.PublicBoard(_boardCreator.CreateBoard(numSides, tilesPerSide));
            _pieceGenerator.SpawnPieces(numSides, tilesPerSide, piecesPerTile);
            new PieceRelease(_pieceGenerator.Citizens, _pieceGenerator.Mandarins, piecesPerTile,
                _container.Board, _gridLocator, OnAllPiecesInPlace).ReleasePieces();

            _bambooFamily.BeginAnimSequence();
        }

        private void OnAllPiecesInPlace()
        {
            VisualReadyEvent?.Invoke(this);
        }

        public void OnMovePieceToNewTileDone(PieceInteractResultData resultData)
        {
        }

        public void OnMoveAllPiecesToPocketDone(PieceInteractResultData resultData)
        {
        }

        public void OnSimulationResult(MoveSimulationOutputData result)
        {
        }

        public void OnSimulationProgress(MoveSimulationOutputData result)
        {
        }
    }
}