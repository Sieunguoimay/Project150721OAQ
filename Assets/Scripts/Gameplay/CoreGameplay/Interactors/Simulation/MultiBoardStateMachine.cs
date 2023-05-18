﻿using System.Collections.Generic;
using System.Linq;
using Common;
using Common.DecisionMaking;
using SNM;
using UnityEngine;

namespace Gameplay.CoreGameplay.Interactors.Simulation
{
    public class MultiBoardStateMachine : BaseBoardStateMachine
    {
        private readonly IStateMachine[] _stateMachines;

        private readonly List<IStateMachine> _completedStateMachines = new();
        private readonly List<IStateMachine> _incompleteStateMachines = new();

        private int _actionCompletedCount;

        public MultiBoardStateMachine(IReadOnlyList<IMoveMaker> executors)
        {
            _stateMachines = new IStateMachine[executors.Count];
            for (var i = 0; i < executors.Count; i++)
            {
                _stateMachines[i] = SetupStateMachine(executors[i]);
            }
        }

        protected override void OnHandleIdleStateEnter(IStateMachine stateMachine)
        {
            _completedStateMachines.Add(stateMachine);
        }

        protected override void HandleAnyActionComplete(IStateMachine stateMachine)
        {
            _actionCompletedCount++;
            
            if (_incompleteStateMachines.Count == _actionCompletedCount)
            {
                if (_completedStateMachines.Count == _stateMachines.Length)
                {
                    // InvokeEndEvent();
                    //Debug.Log($"StateMachine Ended {_completedStateMachines.Count} == {_stateMachines.Length} ");
                    PublicExecutor.Instance.ExecuteInNextFrame(InvokeEndEvent);
                }
                else
                {
                    //Debug.Log($"StateMachine Action Done {_actionCompletedCount} == {_incompleteStateMachines.Count}");
                    PublicExecutor.Instance.ExecuteInNextFrame(InnerNextAction);
                }
            }
            
        }

        protected override void OnNextAction()
        {
            _completedStateMachines.Clear();
            InnerNextAction();
        }

        private void InnerNextAction()
        {
            _actionCompletedCount = 0;
            _incompleteStateMachines.Clear();
            _incompleteStateMachines.AddRange(_stateMachines.Where(sm=>!_completedStateMachines.Contains(sm)));
            foreach (var t in _incompleteStateMachines)
            {
                // if (_completedStateMachines.Contains(t)) continue;
                (t.CurrentState as BaseBoardState)?.NextAction();
            }
        }
    }
}