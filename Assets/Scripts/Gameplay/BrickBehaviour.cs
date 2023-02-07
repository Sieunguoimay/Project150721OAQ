using System;
using System.Linq;
using Common.DecisionMaking;
using Framework.Entities.Variable.Boolean;
using Gameplay.Entities.Stage;
using Gameplay.Entities.Stage.StageSelector;
using UnityEngine;

namespace Gameplay
{
    public class BrickBehaviour : MonoBehaviour
    {
        [SerializeField] private StageEntityView stageView;
        [SerializeField] private BooleanEntityManualView stageUnlock;
        [SerializeField] private StageSelectorEntityView stageSelector;
        [SerializeField] private StateMachineCreator stateMachine;

        private int _state2;
        private int _stateBlend2To3;
        private int _state3;
        private int _stateBlend3To2;

        private void Start()
        {
            _state2 = stateMachine.GetIndex("state_2");
            _state3 = stateMachine.GetIndex("state_3");
            _stateBlend2To3 = stateMachine.GetIndex("state_blend_2_3");
            _stateBlend3To2 = stateMachine.GetIndex("state_blend_3_2");
        }

        public void OnClick()
        {
            var unlocked = stageUnlock.Entity.Value;
            if (unlocked)
            {
                stageSelector.Entity.Select(stageView.Entity);
            }
            else
            {
                stageUnlock.Entity.SetValue(true);
            }
        }

        public void OnSelectionChanged()
        {
            if (!stageUnlock.Entity.Value) return;
            if (stageSelector.Entity.SelectedStage == stageView.Entity)
            {
                //TransitionToSelectedState
                ForceTransitionToState(_state3, _stateBlend2To3);
            }
            else
            {
                //TransitionToUnselectedState
                ForceTransitionToState(_state2, _stateBlend3To2);
            }
        }

        private void ForceTransitionToState(int state, int blendToState)
        {
            if (
                stateMachine.StateMachine.CurrentStateIndex != state &&
                stateMachine.StateMachine.CurrentStateIndex != blendToState)
            {
                stateMachine.StateMachine.ChangeState(blendToState);
            }
        }
    }
}