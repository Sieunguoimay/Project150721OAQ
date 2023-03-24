using System.Linq;
using Gameplay.CoreGameplay.Interactors.OptionSystem;
using Gameplay.Visual.Views;
using UnityEngine;

namespace Gameplay.CoreGameplay.Interactors.MoveDecisionMaking
{
    public class PlayerDecisionMaking : IBoardActionDecisionMaking, OptionQueueIterator.IOptionQueueIterationHandler
    {
        private readonly InteractSystem _interactSystem;
        private IBoardActionDecisionMakingResultHandler _driver;
        private OptionQueueIterator _queueIterator;
        private DecisionMakingData _optionQueue;

        public PlayerDecisionMaking(InteractSystem interactSystem)
        {
            _interactSystem = interactSystem;
        }

        public void MakeDecision(DecisionMakingData optionQueue, IBoardActionDecisionMakingResultHandler driver)
        {
            _optionQueue = optionQueue;
            _driver = driver;
            _queueIterator = new OptionQueueIterator(optionQueue.OptionQueue, this);
            _queueIterator.NextOptionItem();
        }

        public void ForceEnd()
        {
            _interactSystem.Dismiss();
        }

        public void OnOptionsQueueEmpty()
        {
            _driver.OnDecisionResult(IBoardActionDecisionMaking.CreateResultData(_optionQueue));
        }

        public void HandleOptionItem()
        {
            switch (_queueIterator.CurrentOptionItem)
            {
                case TileOptionItem:
                    HandleTilesOption();
                    break;
                case DirectionOptionItem:
                    HandleDirectionsOption();
                    break;
                // case DynamicOptionItem:
                //     ApplyRandom();
                //     break;
            }
        }

        private void HandleTilesOption()
        {
            var values = _queueIterator.CurrentOptionItem.Values.Where(v => _queueIterator.OptionQueue.Options.All(o => o.SelectedValue != v));

            _interactSystem.ShowTileSelector(values.Select(v => ((IntegerOptionValue)v).Value), tileIndex =>
            {
                _queueIterator.CurrentOptionItem.ApplySelectedValue(_queueIterator.CurrentOptionItem.Values.FirstOrDefault(v => ((IntegerOptionValue)v).Value == tileIndex));

                _queueIterator.NextOptionItem();
            });
        }

        private void HandleDirectionsOption()
        {
            _interactSystem.ShowActionChooser(direction =>
            {
                _queueIterator.CurrentOptionItem.ApplySelectedValue(_queueIterator.CurrentOptionItem.Values.FirstOrDefault(v => ((BooleanOptionValue)v).Value == direction));
                _queueIterator.NextOptionItem();
            });
        }

        private void ApplyRandom()
        {
            var valueIndex = Random.Range(0, _queueIterator.CurrentOptionItem.Values.Length);
            _queueIterator.CurrentOptionItem.ApplySelectedValue(_queueIterator.CurrentOptionItem.Values[valueIndex]);
            _queueIterator.NextOptionItem();
        }
    }
}