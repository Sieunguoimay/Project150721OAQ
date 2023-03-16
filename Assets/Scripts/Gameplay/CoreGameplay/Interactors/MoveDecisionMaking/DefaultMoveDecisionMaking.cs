using System.Linq;
using Common;
using SNM;
using UnityEngine;

namespace Gameplay.CoreGameplay.Interactors.MoveDecisionMaking
{
    public class DefaultDecisionMaking : IMoveDecisionMaking
    {
        private IMoveDecisionMakingResultHandler _driver;
        private Coroutine _coroutine;
        private OptionQueueIterator _queueIterator;

        public void MakeDecision(OptionQueue optionQueue, IMoveDecisionMakingResultHandler driver)
        {
            _driver = driver;
            _queueIterator = new OptionQueueIterator(optionQueue, null);

            foreach (var unused in _queueIterator)
            {
                HandleMoveOptionItem(unused);
            }

            _coroutine = PublicExecutor.Instance.Delay(1f, () =>
            {
                var result = IMoveDecisionMaking.CreateResultData(_queueIterator.OptionQueue);
                _driver.OnDecisionResult(result);
            });
        }

        public void ForceEnd()
        {
            //Only for multi-frame decision making
            PublicExecutor.Instance.StopCoroutine(_coroutine);
        }

        private void HandleMoveOptionItem(OptionItem optionItem)
        {
            switch (optionItem)
            {
                case TileOptionItem:
                    HandleTilesOptionItem();
                    break;
                case DirectionOptionItem:
                    HandleDirectionsOptionItem();
                    break;
            }
        }

        private void HandleTilesOptionItem()
        {
            var values = _queueIterator.CurrentOptionItem.Values
                .Where(v => _queueIterator.OptionQueue.Options.All(o => o.SelectedValue != v)).ToArray();

            _queueIterator.CurrentOptionItem.SelectedValue = values[Random.Range(0, values.Length)];
        }

        private void HandleDirectionsOptionItem()
        {
            _queueIterator.CurrentOptionItem.SelectedValue =
                _queueIterator.CurrentOptionItem.Values[Random.Range(0, _queueIterator.CurrentOptionItem.Values.Length)];
        }
    }
}