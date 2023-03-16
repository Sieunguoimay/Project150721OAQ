using System.Collections;
using System.Collections.Generic;

namespace Gameplay.CoreGameplay.Interactors.MoveDecisionMaking
{
    public class OptionQueue
    {
        public OptionItem[] Options;
        public int TurnIndex;
    }

    public class OptionItem
    {
        public OptionValue[] Values;
        public OptionValue SelectedValue;
    }

    public abstract class OptionValue
    {
        protected readonly object DynamicValue;

        protected OptionValue(object value)
        {
            DynamicValue = value;
        }
    }

    public class BooleanOptionValue : OptionValue
    {
        public bool Value => DynamicValue is true;

        public BooleanOptionValue(object value) : base(value)
        {
        }
    }

    public class IntegerOptionValue : OptionValue
    {
        public int Value => DynamicValue is int value ? value : 0;

        public IntegerOptionValue(object value) : base(value)
        {
        }
    }

    public class OptionQueueIterator : IEnumerator<OptionItem>, IEnumerable<OptionItem>
    {
        public OptionQueue OptionQueue { get; }
        public OptionItem CurrentOptionItem { get; private set; }

        private int _optionQueueIndex;
        private readonly IOptionQueueIterationHandler _handler;

        public OptionQueueIterator(OptionQueue optionQueue, IOptionQueueIterationHandler handler)
        {
            OptionQueue = optionQueue;
            _handler = handler;
        }

        public bool NextOptionItem()
        {
            if (_optionQueueIndex >= OptionQueue.Options.Length)
            {
                _handler?.OnOptionsQueueEmpty();
                return false;
            }

            CurrentOptionItem = OptionQueue.Options[_optionQueueIndex++];
            _handler?.HandleOptionItem();
            return true;
        }

        public interface IOptionQueueIterationHandler
        {
            void OnOptionsQueueEmpty();
            void HandleOptionItem();
        }

        private void InnerReset()
        {
            _optionQueueIndex = -1;
            CurrentOptionItem = null;
        }

        #region IEnumerator<OptionItem>, IEnumerable<OptionItem>

        public bool MoveNext()
        {
            return NextOptionItem();
        }

        public void Reset()
        {
            InnerReset();
        }

        public OptionItem Current => CurrentOptionItem;

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }

        public IEnumerator<OptionItem> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }

        #endregion
    }
}