using System.Linq;
using Common;
using Gameplay.CoreGameplay.Interactors.OptionSystem;
using SNM;
using UnityEngine;

namespace Gameplay.CoreGameplay.Interactors.MoveDecisionMaking
{
    public class DefaultDecisionMaker : IDecisionMaker
    {
        private IDecisionMakingResultHandler _driver;
        private Coroutine _coroutine;
        private OptionQueueIterator _queueIterator;

        public void MakeDecision(DecisionMakingData optionQueue, IDecisionMakingResultHandler driver)
        {
            _driver = driver;
            _queueIterator = new OptionQueueIterator(optionQueue.OptionQueue, null);

            foreach (var unused in _queueIterator)
            {
                HandleMoveOptionItem(unused);
            }

            _coroutine = PublicExecutor.Instance.Delay(1f, () =>
            {
                var result = IDecisionMaker.CreateResultData(optionQueue);
                _driver.OnDecisionResult(result);
            });
        }

        public void Cancel()
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
                    ApplyRandom();
                    break;
                // case DynamicOptionItem:
                //     ApplyRandom();
                //     break;
            }
        }

        private void HandleTilesOptionItem()
        {
            var values = _queueIterator.CurrentOptionItem.Values
                .Where(v => _queueIterator.OptionQueue.Options.All(o => o.SelectedValue != v)).ToArray();

            _queueIterator.CurrentOptionItem.ApplySelectedValue(values[Random.Range(0, values.Length)]);
        }

        private void ApplyRandom()
        {
            _queueIterator.CurrentOptionItem.ApplySelectedValue(
                _queueIterator.CurrentOptionItem.Values[Random.Range(0, _queueIterator.CurrentOptionItem.Values.Length)]);
        }
    }
}