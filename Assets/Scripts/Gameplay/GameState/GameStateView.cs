using UnityEngine;

namespace Gameplay.GameState
{
    public class GameStateView : MonoBehaviour
    {
        [SerializeField] private GameStateController gameStateController;
        public IGameState GameState => gameStateController.GameState;
        public bool IsInMenu => GameState != null && GameState.CurrentState == Gameplay.GameState.GameState.State.InMenu;
    }
}