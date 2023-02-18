using Gameplay.Board;
using UnityEngine;

namespace Test
{
    public class TestBoardStateDriver:MonoBehaviour
    {
        private BoardStateDriver _boardStateDriver;

        [ContextMenu("NextAction")]
        private void NextAction()
        {
            _boardStateDriver.NextAction();
        }

        [ContextMenu("Next5Actions")]
        private void Next5Actions()
        {
            if (_boardStateDriver == null)
            {
                _boardStateDriver = new BoardStateDriver(new BoardActionExecutor());
                _boardStateDriver.EndEvent += OnEndEvent;
            }
            
            for (var i = 0; i < 5; i++)
            {
                _boardStateDriver.NextAction();
            }
        }

        private static void OnEndEvent(IBoardStateDriver obj)
        {
            Debug.Log("End: Change player");
        }
    }
}