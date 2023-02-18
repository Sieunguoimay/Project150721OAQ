﻿using System;
using System.Collections.Generic;
using Common.DecisionMaking;

namespace Gameplay.Board
{
    public class ConcurrentBoardStateDriver : BaseBoardStateDriver
    {
        private readonly IStateMachine[] _stateMachines;
        private int _anyActionCompleteCount;

        private readonly List<IStateMachine> _completedStateMachines = new();
        public ConcurrentBoardStateDriver(IReadOnlyList<IBoardActionExecutor> executors)
        {
            _stateMachines = new IStateMachine[executors.Count];
            for (var i = 0; i < executors.Count; i++)
            {
                _stateMachines[i] = new StateMachine();
                SetupStateMachine(_stateMachines[i], executors[i]);
            }
        }

        protected override void OnHandleIdleStateEnter(IStateMachine stateMachine)
        {
            _completedStateMachines.Add(stateMachine);
            
            if (_completedStateMachines.Count == _stateMachines.Length)
            {
                InvokeEndEvent();
            }
        }

        protected override void HandleAnyActionComplete()
        {
            _anyActionCompleteCount++;
            if (_anyActionCompleteCount == _stateMachines.Length - _completedStateMachines.Count)
            {
                InnerNextAction();
            }
        }

        public override void NextAction()
        {
            _completedStateMachines.Clear();
            InnerNextAction();
        }

        private void InnerNextAction()
        {
            _anyActionCompleteCount = 0;
            foreach (var t in _stateMachines)
            {
                if (_completedStateMachines.Contains(t)) continue;
                (t.CurrentState as BaseBoardState)?.NextAction();
            }
        }
    }
}