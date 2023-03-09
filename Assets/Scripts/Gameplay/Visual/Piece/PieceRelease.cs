using System;
using System.Collections.Generic;
using Gameplay.CoreGameplay.Interactors;
using Gameplay.Helpers;
using UnityEngine;

namespace Gameplay.Visual.Piece
{
    public class PieceRelease
    {
        private readonly IReadOnlyList<Citizen> _citizens;
        private readonly IReadOnlyList<Mandarin> _mandarins;
        private readonly int _numCitizensPerTile;
        private readonly Action _onAllInPlace;
        private readonly Board.BoardVisual _boardVisual;
        private readonly GridLocator _gridLocator;

        public PieceRelease(IReadOnlyList<Citizen> citizens, IReadOnlyList<Mandarin> mandarins, int numCitizensPerTile,
            Board.BoardVisual boardVisual, GridLocator gridLocator, Action done)
        {
            _citizens = citizens;
            _mandarins = mandarins;
            _numCitizensPerTile = numCitizensPerTile;
            _boardVisual = boardVisual;
            _gridLocator = gridLocator;
            _onAllInPlace = done;
        }

        public void ReleasePieces(RefreshData refreshData)
        {
            
        }
        
        public void ReleasePieces()
        {
            // for (var i = 0; i < _boardVisual.SideVisuals.Count; i++)
            // {
            //     var tg = _boardVisual.SideVisuals[i];
            //     var numTilesPerSide = tg.CitizenTiles.Count;
            //     for (var j = 0; j < numTilesPerSide; j++)
            //     {
            //         var ct = tg.CitizenTiles[j];
            //         for (var k = 0; k < _numCitizensPerTile; k++)
            //         {
            //             var index = i * numTilesPerSide * _numCitizensPerTile + j * _numCitizensPerTile + k;
            //             var p = _citizens[index];
            //
            //             ct.AddPiece(p);
            //
            //             var delay = k * 0.1f;
            //             var gridIndex = Mathf.Max(0, ct.HeldPieces.Count - 1);
            //             var position = _gridLocator.GetPositionAtCellIndex(ct.transform, gridIndex);
            //             p.Animator.StraightMove(position, index == _citizens.Count - 1 ? _onAllInPlace : null, delay);
            //         }
            //     }
            //
            //     _mandarins[i].transform.position = _gridLocator.GetPositionAtCellIndex(tg.MandarinTile.transform, 0);
            //     tg.MandarinTile.AddPiece(_mandarins[i]);
            // }
        }
    }
}