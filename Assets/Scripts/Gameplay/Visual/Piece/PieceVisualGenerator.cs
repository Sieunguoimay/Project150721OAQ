using System;
using System.Collections.Generic;
using Framework.DependencyInversion;
using UnityEngine;

namespace Gameplay.Visual.Piece
{
    public class PieceVisualGenerator : MonoBehaviour
    {
        [SerializeField] private Mandarin mandarinPrefab;
        [SerializeField] private Citizen citizenPrefab;

        private readonly List<Mandarin> _mandarins = new();
        private readonly List<Citizen> _citizens  = new();

        private IEnumerable<PieceVisual> AllPieces
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

            _mandarins.Clear();
            _citizens.Clear();
        }

        public Citizen[] SpawnCitizens(int amount)
        {
            var pieces = SpawnPieces(citizenPrefab, amount);
            _citizens.AddRange(pieces);
            return pieces;
        }

        public Mandarin[] SpawnMandarins(int amount)
        {
            var mandarins = SpawnPieces(mandarinPrefab, amount);
            _mandarins.AddRange(mandarins);
            return mandarins;
        }

        private TPiece[] SpawnPieces<TPiece>(TPiece prefab, int amount) where TPiece : Component
        {
            var citizens = new TPiece[amount];
            for (var i = 0; i < citizens.Length; i++)
            {
                citizens[i] = Instantiate(prefab, transform);
            }

            return citizens;
        }
    }
}