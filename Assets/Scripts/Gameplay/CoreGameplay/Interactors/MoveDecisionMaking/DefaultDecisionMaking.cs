using System.Linq;
using Common;
using Gameplay.OptionSystem;
using SNM;
using UnityEngine;

namespace Gameplay.CoreGameplay.Interactors.MoveDecisionMaking
{
    public class DefaultDecisionMaking : IBoardActionDecisionMaking
    {
        private IBoardActionDecisionMakingResultHandler _driver;
        private Coroutine _coroutine;
        private OptionQueueIterator _queueIterator;

        public void MakeDecision(OptionQueue optionQueue, IBoardActionDecisionMakingResultHandler driver)
        {
            _driver = driver;
            _queueIterator = new OptionQueueIterator(optionQueue, null);

            foreach (var unused in _queueIterator)
            {
                HandleMoveOptionItem(unused);
            }

            _coroutine = PublicExecutor.Instance.Delay(1f, () =>
            {
                var result = IBoardActionDecisionMaking.CreateResultData(_queueIterator.OptionQueue);
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
                    ApplyRandom();
                    break;
                case DynamicOptionItem:
                    ApplyRandom();
                    break;
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