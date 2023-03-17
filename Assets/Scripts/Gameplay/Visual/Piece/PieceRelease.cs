using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Gameplay.CoreGameplay.Interactors;
using Gameplay.Helpers;
using Gameplay.Visual.Views;
using SNM;
using UnityEngine;

namespace Gameplay.Visual.Piece
{
    public class PieceRelease
    {
        private readonly PieceVisualGenerator _pieceVisualGenerator;
        private readonly Board.BoardVisual _boardVisual;
        private readonly GridLocator _gridLocator;
        private readonly Action _onAllInPlace;

        public PieceRelease(PieceVisualGenerator pieceVisualGenerator, Board.BoardVisual boardVisual, 
            GridLocator gridLocator, Action done)
        {
            _pieceVisualGenerator = pieceVisualGenerator;
            _boardVisual = boardVisual;
            _gridLocator = gridLocator;
            _onAllInPlace = done;
        }

        public void ReleasePieces(RefreshData refreshData)
        {
            var allCitizens = SpawnCitizens(refreshData);
            var allMandarins = SpawnMandarins(refreshData);

            var citizenCount = 0;
            var mandarinCount = 0;

            for (var i = 0; i < refreshData.PiecesInTiles.Length; i++)
            {
                var amount = refreshData.PiecesInTiles[i].CitizenPiecesCount;
                if (amount > 0)
                {
                    var citizens = allCitizens[citizenCount..(citizenCount + amount)];
                    ReleasePiecesToTile(citizens, i);
                    citizenCount += amount;
                }

                amount = refreshData.PiecesInTiles[i].MandarinPiecesCount;
                if (amount > 0)
                {
                    var mandarins = allMandarins[mandarinCount..(mandarinCount + amount)];
                    ReleasePiecesToTile(mandarins, i);
                    mandarinCount += amount;
                }
            }

            PublicExecutor.Instance.Delay(2, _onAllInPlace);
        }

        private void ReleasePiecesToTile(IReadOnlyList<PieceVisual> releasedPieces, int tileIndex)
        {
            var tile = _boardVisual.TileVisuals[tileIndex];

            PiecesMovingRunner.MovePieces(_gridLocator, releasedPieces, tile, releasedPieces.Count);

            tile.AddPieces(releasedPieces);
        }

        private Mandarin[] SpawnMandarins(RefreshData refreshData)
        {
            var totalMandarins = refreshData.PiecesInTiles.Sum(p => p.MandarinPiecesCount);
            var allMandarins = _pieceVisualGenerator.SpawnMandarins(totalMandarins);
            return allMandarins;
        }

        private Citizen[] SpawnCitizens(RefreshData refreshData)
        {
            var totalCitizens = refreshData.PiecesInTiles.Sum(p => p.CitizenPiecesCount);
            var allCitizens = _pieceVisualGenerator.SpawnCitizens(totalCitizens);
            return allCitizens;
        }
    }
}