using System.Linq;
using Common;
using SNM;
using UnityEngine;

namespace Gameplay.CoreGameplay.Interactors.MoveDecisionMaking
{
    public class DefaultMoveDecisionMaking : IMoveDecisionMaking, MoveOptionQueueIterator.IMoveOptionQueueIterationHandler
    {
        private IMoveDecisionMakingResultHandler _driver;
        private Coroutine _coroutine;
        private MoveOptionQueueIterator _queueIterator;

        public void MakeDecision(MoveOptionQueue moveOptionQueue, IMoveDecisionMakingResultHandler driver)
        {
            _driver = driver;
            _queueIterator = new MoveOptionQueueIterator(moveOptionQueue, this);
            _queueIterator.DequeueNextOptionItem();
        }

        public void ForceEnd()
        {
            //Only for multi-frame decision making
            PublicExecutor.Instance.StopCoroutine(_coroutine);
        }

        public void OnOptionsQueueEmpty()
        {
            _coroutine = PublicExecutor.Instance.Delay(1f,
                () => { _driver.OnDecisionResult(IMoveDecisionMaking.CreateResultData(_queueIterator.MoveOptionQueue)); });
        }

        public void HandleTilesOption()
        {
            var values = _queueIterator.CurrentOptionItem.Values.Where(v => _queueIterator.MoveOptionQueue.Options.All(o => o.SelectedValue != v));

            _queueIterator.CurrentOptionItem.SelectedValue = values.ToArray()[Random.Range(0, _queueIterator.CurrentOptionItem.Values.Length)];
            _queueIterator.DequeueNextOptionItem();
        }

        public void HandleDirectionsOption()
        {
            _queueIterator.CurrentOptionItem.SelectedValue = _queueIterator.CurrentOptionItem.Values[Random.Range(0, _queueIterator.CurrentOptionItem.Values.Length)];
            _queueIterator.DequeueNextOptionItem();
        }
    }
}