﻿using System;
using Framework.DependencyInversion;
using Framework.Resolver;

namespace Gameplay.GameState
{
    public class GameStateView : InjectableMonoBehaviour
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