using System;
using Gameplay.CoreGameplay.Interactors;
using Gameplay.Helpers;
using Gameplay.Visual.BambooStick;
using Gameplay.Visual.Board;
using Gameplay.Visual.Piece;
using UnityEngine;

namespace Gameplay.Visual.Views
{
    public class BoardVisualGenerator : MonoBehaviour
    {
        [SerializeField] private BoardVisualGeneratorRepresenter representer;
        [SerializeField] private BambooFamilyManager bambooFamily;
        [SerializeField] private BoardVisualCreator boardVisualCreator;
        [SerializeField] private PieceVisualGenerator pieceVisualGenerator;
        [SerializeField] private GridLocator gridLocator;

        public BoardVisual BoardVisual { get; private set; }
        public event Action<BoardVisualGenerator> VisualReadyEvent;

        private void Awake()
        {
            representer.SetAuthor(this);
        }

        public void GenerateBoardVisual(RefreshData refreshData)
        {
            var numSides = refreshData.BoardData.NumSides;
            var tilesPerSide = refreshData.BoardData.TilesPerSide;
            var piecesPerTile = refreshData.BoardData.PiecesPerTile;

            BoardVisual = boardVisualCreator.CreateBoard(numSides, tilesPerSide);

            // _pieceGenerator.SpawnPieces(numSides, tilesPerSide, piecesPerTile);

            new PieceRelease(pieceVisualGenerator, BoardVisual, gridLocator, OnAllPiecesInPlace)
                .ReleasePieces(refreshData);

            bambooFamily.BeginAnimSequence(BoardVisual);
        }

        public void Cleanup()
        {
            bambooFamily.ResetAll();
            pieceVisualGenerator.DeletePieces();
            BoardVisualCreator.DeleteBoard(BoardVisual);
        }

        private void OnAllPiecesInPlace()
        {
            VisualReadyEvent?.Invoke(this);
        }
    }
}