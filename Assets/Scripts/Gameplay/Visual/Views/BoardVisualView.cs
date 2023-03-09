using System;
using Gameplay.BambooStick;
using Gameplay.CoreGameplay.Gateway;
using Gameplay.CoreGameplay.Interactors;
using Gameplay.Helpers;
using Gameplay.Visual.Board;
using Gameplay.Visual.Piece;

namespace Gameplay.Visual.Views
{
    public class BoardVisualView : BaseGenericDependencyInversionUnit<BoardVisualView>
    {
        private BambooFamilyManager _bambooFamily;
        private BoardCreator _boardCreator;
        private PieceGenerator _pieceGenerator;
        private GridLocator _gridLocator;
        public BoardVisual BoardVisual { get; private set; }
        public event Action<BoardVisualView> VisualReadyEvent;
        protected override void OnSetupDependencies()
        {
            base.OnSetupDependencies();
            _bambooFamily = Resolver.Resolve<BambooFamilyManager>();
            _boardCreator = Resolver.Resolve<BoardCreator>();
            _pieceGenerator = Resolver.Resolve<PieceGenerator>();
            _gridLocator = Resolver.Resolve<GridLocator>();
        }

        public void RefreshVisual(RefreshData refreshData)
        {
            GenerateBoardVisual(refreshData);
        }

        private void GenerateBoardVisual(RefreshData refreshData)
        {
            var numSides = refreshData.BoardData.NumSides;
            var tilesPerSide = refreshData.BoardData.TilesPerSide;
            var piecesPerTile = refreshData.BoardData.PiecesPerTile;

            BoardVisual = _boardCreator.CreateBoard(numSides, tilesPerSide);

            // _pieceGenerator.SpawnPieces(numSides, tilesPerSide, piecesPerTile);

            new PieceRelease(_pieceGenerator.Citizens, _pieceGenerator.Mandarins, piecesPerTile,
                BoardVisual, _gridLocator, OnAllPiecesInPlace).ReleasePieces(refreshData);

            _bambooFamily.BeginAnimSequence(BoardVisual);
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