using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Visual.Piece
{
    public class PieceGenerator : BaseGenericDependencyInversionUnit<PieceGenerator>
    {
        [SerializeField] private Mandarin mandarinPrefab;
        [SerializeField] private Citizen citizenPrefab;

        public Mandarin[] Mandarins { get; private set; }
        public Citizen[] Citizens { get; private set; }

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

        public void SpawnPieces(int groups, int tilesPerGroup, int numCitizens)
        {
            Mandarins = SpawnPieces(mandarinPrefab, groups);
            Citizens = SpawnPieces(citizenPrefab, groups * tilesPerGroup * numCitizens);
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