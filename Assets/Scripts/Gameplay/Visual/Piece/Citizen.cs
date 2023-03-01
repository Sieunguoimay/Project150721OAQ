using UnityEngine;

namespace Gameplay.Visual.Piece
{
    [SelectionBase]
    public class Citizen : Piece
    {
        [SerializeField] private CitizenAnimator animator;
        public CitizenAnimator Animator => animator;
    }
}