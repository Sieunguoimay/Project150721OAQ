using Gameplay.Board;
using UnityEngine;

namespace Test
{
    public class TestBoardStateDriver : MonoBehaviour
    {
        private BoardStateMachine _boardStateMachine;

        [ContextMenu("NextAction")]
        private void NextAction()
        {
            _boardStateMachine.NextAction();
        }

        [ContextMenu("Next5Actions")]
        private void Next5Actions()
        {
            if (_boardStateMachine == null)
            {
                _boardStateMachine = new BoardStateMachine(new MoveMaker(null, 0));
                _boardStateMachine.EndEvent += OnEndEvent;
            }

            for (var i = 0; i < 5; i++)
            {
                _boardStateMachine.NextAction();
            }
        }

        private static void OnEndEvent(IBoardStateDriver obj)
        {
            Debug.Log("End: Change player");
        }
    }
}