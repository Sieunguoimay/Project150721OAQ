using System;
using System.Collections.Generic;
using Common.Activity;
using Gameplay.Piece.Activities;
using UnityEngine;

namespace Gameplay.Piece
{
    public class PieceManager : MonoControlUnitBase<PieceManager>
    {
        [SerializeField] private Mandarin mandarinPrefab;
        [SerializeField] private Citizen citizenPrefab;

        private Mandarin[] _mandarins;
        private Citizen[] _citizens;

        private int _numCitizensPerTile;
        private IEnumerable<Piece> AllPieces
        {
            get
            {
                foreach (var m in _mandarins)
                {
                    yield return m;
                }

                foreach (var c in _citizens)
                {
                    yield return c;
                }
            }
        }

        public void DeletePieces()
        {
            foreach (var p in AllPieces)
            {
                Destroy(p.gameObject);
            }

            _mandarins = null;
            _citizens = null;
        }

        public void SpawnPieces(int groups, int tilesPerGroup, int numCitizens)
        {
            _numCitizensPerTile = numCitizens;
            _mandarins = new Mandarin[groups];
            _citizens = new Citizen[groups * tilesPerGroup * numCitizens];
            for (var i = 0; i < groups; i++)
            {
                for (var j = 0; j < tilesPerGroup; j++)
                {
                    for (var k = 0; k < numCitizens; k++)
                    {
                        _citizens[i * tilesPerGroup * numCitizens + j * numCitizens + k] = Instantiate(citizenPrefab, transform);
                    }
                }

                _mandarins[i] = Instantiate(mandarinPrefab, transform);
            }
        }

        public void ReleasePieces(Action onAllInPlace, Board.Board board)
        {
            for (var i = 0; i < board.Sides.Length; i++)
            {
                var tg = board.Sides[i];
                var numTilesPerSide = tg.CitizenTiles.Length;
                for (var j = 0; j < numTilesPerSide; j++)
                {
                    var ct = tg.CitizenTiles[j];
                    for (var k = 0; k < _numCitizensPerTile; k++)
                    {
                        var index = i * numTilesPerSide * _numCitizensPerTile + j * _numCitizensPerTile + k;
                        var p = _citizens[index];

                        ct.AddPiece(p);

                        var delay = k * 0.1f;
                        var position = ct.GetPositionInFilledCircle(Mathf.Max(0, ct.HeldPieces.Count - 1));
                        p.ActivityQueue.Add(new ActivityAnimation(p.Animator, LegHashes.sit_down));
                        p.ActivityQueue.Add(delay > 0 ? new ActivityDelay(delay) : null);
                        p.ActivityQueue.Add(new ActivityFlocking(p.FlockingConfigData, position, p.transform, null));
                        p.ActivityQueue.Add(index == _citizens.Length - 1 ? new ActivityCallback(onAllInPlace) : null);
                        p.ActivityQueue.Begin();
                    }
                }

                _mandarins[i].transform.position = tg.MandarinTile.GetPositionInFilledCircle(0);
                tg.MandarinTile.SetMandarin(_mandarins[i]);
            }
        }
    }
}