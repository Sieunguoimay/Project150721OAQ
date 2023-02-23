using System;
using System.Linq;
using Common.DecisionMaking;
using UnityEngine;

namespace Gameplay.Board
{
    public interface IBoardStateDriver
    {
        void NextAction();
        event Action<IBoardStateDriver> EndEvent;
    }

    public abstract class BaseBoardStateDriver : IBoardStateDriver
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

        protected void SetupStateMachine(IStateMachine stateMachine, IMoveMaker executor)
        {
            var boardStates = new BaseBoardState[]
            {
                new StateIdle(stateMachine, HandleIdleStateEnter),
                new GraspATile(stateMachine),
                new KeepDropping(stateMachine),
                new SlamAndEat(stateMachine)
            };

            stateMachine.SetStates(boardStates.Select(s => s as IState).ToArray());

            foreach (var s in boardStates)
            {
                s.Setup(HandleAnyActionComplete, executor);
            }
        }

        private void HandleIdleStateEnter(BaseBoardState baseBoardState)
        {
            OnHandleIdleStateEnter(baseBoardState.StateMachine);
        }
        protected abstract void OnHandleIdleStateEnter(IStateMachine stateMachine);

        protected abstract void HandleAnyActionComplete();

        public abstract void NextAction();

        protected void InvokeEndEvent() => EndEvent?.Invoke(this);

        public event Action<IBoardStateDriver> EndEvent;


        protected abstract class BaseBoardState : BaseState
        {
            protected IMoveMaker Executor { get; private set; }
            private Action _actionCompleteHandler;

            protected BaseBoardState(IStateMachine stateMachine) : base(stateMachine)
            {
            }

            protected abstract void OnSetup();
            public abstract void NextAction();

            public void Setup(Action actionCompleteHandler, IMoveMaker executor)
            {
                Executor = executor;
                _actionCompleteHandler = actionCompleteHandler;
                OnSetup();
            }

            protected void InvokeActionCompleteHandler()
            {
                _actionCompleteHandler?.Invoke();
            }
        }

        private class StateIdle : BaseBoardState
        {
            private GraspATile _graspATile;

            private readonly Action<BaseBoardState> _stateEnterHandler;

            public StateIdle(IStateMachine stateMachine, Action<BaseBoardState> stateEnterHandler) : base(stateMachine)
            {
                _stateEnterHandler = stateEnterHandler;
            }

            protected override void OnEnter()
            {
                Debug.Log("Perform Action: Idle");
                _stateEnterHandler?.Invoke(this);
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
                StateMachine.ChangeState(_graspATile);
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
                if (Executor.IsValidGrasp())
                {
                    Debug.Log("Perform Action: Grasp a tile");
                    Executor.Grasp(InvokeActionCompleteHandler);
                }
                else
                {
                    StateMachine.ChangeState(_stateIdle);
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
                StateMachine.ChangeState(_keepDropping);
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
                Drop();
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
                if (!Drop())
                {
                    StateMachine.ChangeState(GetNextState());
                }
            }

            private IState GetNextState()
            {
                return Executor.GetSuccessStateAfterDrop(_stateIdle, _slamAndEat, _graspATile);
            }

            private bool Drop()
            {
                if (!Executor.CanDrop()) return false;
                Debug.Log($"Perform action: Drop a piece");
                Executor.Drop(InvokeActionCompleteHandler);
                return true;
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
                if (_slam)
                {
                    _slam = false;
                    Debug.LogError("Perform Action: Slam");
                    Executor.Slam(InvokeActionCompleteHandler);
                }
                else
                {
                    if (_eat)
                    {
                        _eat = false;
                        Debug.LogError("Perform Action: Eat");
                        Executor.Eat(InvokeActionCompleteHandler);
                    }
                    else
                    {
                        if (CanEatMore())
                        {
                            //Move next
                            _slam = true;
                            _eat = true;
                            SlamOrEat();
                        }
                        else
                        {
                            StateMachine.ChangeState(_stateIdle);
                        }
                    }
                }
            }

            private bool CanEatMore()
            {
                return Executor.CanEatMore();
            }
        }
    }

    public class BoardStateDriver : BaseBoardStateDriver
    {
        private readonly StateMachine _stateMachine = new();

        public BoardStateDriver(IMoveMaker executor)
        {
            SetupStateMachine(_stateMachine, executor);
        }

        protected override void OnHandleIdleStateEnter(IStateMachine stateMachine)
        {
            InvokeEndEvent();
        }

        protected override void HandleAnyActionComplete()
        {
            NextAction();
        }

        public override void NextAction()
        {
            (_stateMachine.CurrentState as BaseBoardState)?.NextAction();
        }
    }
}