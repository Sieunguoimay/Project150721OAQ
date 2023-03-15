using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Visual.Piece
{
    public class PieceGenerator : BaseGenericDependencyInversionUnit<PieceGenerator>
    {
        [SerializeField] private Mandarin mandarinPrefab;
        [SerializeField] private Citizen citizenPrefab;

        public List<Mandarin> Mandarins { get; private set; } = new();
        public List<Citizen> Citizens { get; private set; } = new();

        private IEnumerable<Piece> AllPieces
        {
            get
            {
                foreach (var m in Mandarins)
                {
                    yield return m;
                }

                foreach (var c in Citizens)
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

            Mandarins = null;
            Citizens = null;
        }

        public Citizen[] SpawnCitizens(int amount)
        {
            var pieces = SpawnPieces(citizenPrefab, amount);
            Citizens.AddRange(pieces);
            return pieces;
        }

        public Mandarin[] SpawnMandarins(int amount)
        {
            var mandarins = SpawnPieces(mandarinPrefab, amount);
            Mandarins.AddRange(mandarins);
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