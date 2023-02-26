using System;
using Framework.Resolver;

namespace Gameplay.GameState
{
    public class GameStateView : BaseInjectable
    {
        [field: System.NonSerialized] public IGameState GameState { get; private set; }
        
        public bool IsInMenu => GameState.CurrentState == Gameplay.GameState.GameState.State.InMenu;
        protected override void OnInject(IResolver resolver)
        {
            base.OnInject(resolver);
            GameState = resolver.Resolve<IGameState>();
        }
    }
}