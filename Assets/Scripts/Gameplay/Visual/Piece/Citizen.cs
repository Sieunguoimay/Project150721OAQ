using UnityEngine;

namespace Gameplay.Visual.Piece
{
    [SelectionBase]
    public class Citizen : PieceVisual
    {
        [SerializeField] private CitizenAnimator animator;
        public CitizenAnimator Animator => animator;
    }
}