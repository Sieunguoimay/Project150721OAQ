using System;
using System.Linq;
using Common.DecisionMaking;
using UnityEngine;

namespace Gameplay.CoreGameplay.Interactors.Simulation
{
    public abstract partial class BaseBoardStateMachine 
    {
        /*
         * Action List:
         * - Grasp a tile
         * - Drop Single Piece into a tile
         * - Slam the empty tile before eating
         * - Eat a tile
         * - Do nothing
        */

        /*
         * State List:
         * - State Idle: Wait For Signal to begin
         * - Grasp a Tile
         * - Keep Dropping till end
         * - Slam & Eat
        */

        /*
         * Interface
         * - AssignStartingCondition(): void
         * - NextAction(): void
         * - EndEvent: Action
         */
        private Action _endHandler;

        public void SetEndHandler(Action endHandler)
        {
            _endHandler = endHandler;
        }

        protected IStateMachine SetupStateMachine(IMoveMaker executor)
        {
            var stateMachine = new StateMachine();
            var stateIdle = new StateIdle(stateMachine);
            var boardStates = new BaseBoardState[]
            {
                stateIdle,
                new GraspATile(stateMachine),
                new KeepDropping(stateMachine),
                new SlamAndEat(stateMachine)
            };

            stateMachine.SetStates(boardStates.Select(s => s as IState).ToArray());

            foreach (var s in boardStates)
            {
                s.Setup(HandleAnyActionComplete, executor);
            }

            stateIdle.SetStateEnterHandler(HandleIdleStateEnter);

            return stateMachine;
        }

        private void HandleIdleStateEnter(BaseBoardState baseBoardState)
        {
            OnHandleIdleStateEnter(baseBoardState.StateMachine);
        }

        protected abstract void OnHandleIdleStateEnter(IStateMachine stateMachine);

        protected abstract void HandleAnyActionComplete(IStateMachine stateMachine);

        public void NextAction()
        {
            OnNextAction();
        }
        protected abstract void OnNextAction();

        protected void InvokeEndEvent() => _endHandler?.Invoke();

    }

    public abstract partial class BaseBoardStateMachine
    {
        protected abstract class BaseBoardState : BaseState
        {
            protected IMoveMaker Executor { get; private set; }
            private Action<IStateMachine> _actionCompleteHandler;

            protected BaseBoardState(IStateMachine stateMachine) : base(stateMachine)
            {
            }

            protected abstract void OnSetup();
            public abstract void NextAction();

            public void Setup(Action<IStateMachine> actionCompleteHandler, IMoveMaker executor)
            {
                Executor = executor;
                _actionCompleteHandler = actionCompleteHandler;
                OnSetup();
            }

            protected void InvokeActionCompleteHandler()
            {
                _actionCompleteHandler?.Invoke(StateMachine);
            }

            protected void InnerChangeState(IState nextState)
            {
                StateMachine.ChangeState(nextState);
            }
        }

        private class StateIdle : BaseBoardState
        {
            private GraspATile _graspATile;

            private Action<BaseBoardState> _stateEnterHandler;

            public StateIdle(IStateMachine stateMachine) : base(stateMachine)
            {
            }

            public void SetStateEnterHandler(Action<BaseBoardState> stateEnterHandler)
            {
                _stateEnterHandler = stateEnterHandler;
            }

            protected override void OnEnter()
            {
                // Debug.Log("Perform Action: Idle");
                _stateEnterHandler?.Invoke(this);
                InvokeActionCompleteHandler();
            }

            protected override void OnExit()
            {
            }

            protected override void OnSetup()
            {
                _graspATile = StateMachine.States.FirstOrDefault(s => s is GraspATile) as GraspATile;
            }

            public override void NextAction()
            {
                InnerChangeState(_graspATile);
            }
        }

        private class GraspATile : BaseBoardState
        {
            private KeepDropping _keepDropping;
            private StateIdle _stateIdle;

            public GraspATile(IStateMachine stateMachine) : base(stateMachine)
            {
            }

            protected override void OnEnter()
            {
                if (Executor.MoveInnerRules.IsThereAnyPiece())
                {
                    // Debug.Log("Perform Action: Grasp a tile");
                    Executor.Grasp(InvokeActionCompleteHandler);
                }
                else
                {
                    InnerChangeState(_stateIdle);
                }
            }

            protected override void OnExit()
            {
            }

            protected override void OnSetup()
            {
                _keepDropping = StateMachine.States.FirstOrDefault(s => s is KeepDropping) as KeepDropping;
                _stateIdle = StateMachine.States.FirstOrDefault(s => s is StateIdle) as StateIdle;
            }

            public override void NextAction()
            {
                InnerChangeState(_keepDropping);
            }
        }

        private class KeepDropping : BaseBoardState
        {
            private StateIdle _stateIdle;
            private SlamAndEat _slamAndEat;
            private GraspATile _graspATile;

            public KeepDropping(IStateMachine stateMachine) : base(stateMachine)
            {
            }

            protected override void OnEnter()
            {
                if (Executor.CanDrop())
                {
                    Executor.Drop(InvokeActionCompleteHandler);
                }
                else
                {
                    Debug.LogError("Can't drop");
                }
            }

            protected override void OnExit()
            {
            }

            protected override void OnSetup()
            {
                _stateIdle = StateMachine.States.FirstOrDefault(s => s is StateIdle) as StateIdle;
                _slamAndEat = StateMachine.States.FirstOrDefault(s => s is SlamAndEat) as SlamAndEat;
                _graspATile = StateMachine.States.FirstOrDefault(s => s is GraspATile) as GraspATile;
            }

            public override void NextAction()
            {
                if (Executor.CanDrop())
                {
                    Executor.Drop(InvokeActionCompleteHandler);
                }
                else
                {
                    InnerChangeState(GetNextState());
                }
            }

            private IState GetNextState()
            {
                if (Executor.MoveInnerRules.HasReachDeadEnd()) return _stateIdle;
                if (Executor.MoveInnerRules.IsGraspable()) return _graspATile;
                if (Executor.MoveInnerRules.IsEatable()) return _slamAndEat;
                throw new Exception("Invalid condition");
            }
        }

        private class SlamAndEat : BaseBoardState
        {
            private StateIdle _stateIdle;
            private bool _slam;
            private bool _eat;

            public SlamAndEat(IStateMachine stateMachine) : base(stateMachine)
            {
            }

            protected override void OnEnter()
            {
                _slam = true;
                _eat = true;
                SlamOrEat();
            }

            protected override void OnExit()
            {
            }

            protected override void OnSetup()
            {
                _stateIdle = StateMachine.States.FirstOrDefault(s => s is StateIdle) as StateIdle;
            }

            public override void NextAction()
            {
                SlamOrEat();
            }

            private void SlamOrEat()
            {
                while (true)
                {
                    if (_slam)
                    {
                        _slam = false;
                        Executor.Slam(InvokeActionCompleteHandler);
                        break;
                    }

                    if (_eat)
                    {
                        _eat = false;
                        Executor.Eat(InvokeActionCompleteHandler);
                        break;
                    }

                    if (!Executor.MoveInnerRules.IsEatable())
                    {
                        InnerChangeState(_stateIdle);
                        break;
                    }

                    //Move next
                    _slam = true;
                    _eat = true;
                }
            }
        }
    }
}