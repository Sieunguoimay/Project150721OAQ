using System;

namespace Framework.Entities.Variable
{
    public interface IVariable<TValue>
    {
        void SetValue(TValue value);
        TValue Value { get; }
        event Action<object, EventArgs> ValueChanged;
    }

    public class Variable<TValue> : IVariable<TValue>
    {
        public TValue Value { get; private set; }
        public event Action<object, EventArgs> ValueChanged;

        public void SetValue(TValue value)
        {
            Value = value;
            ValueChanged?.Invoke(this, new ValueChangedEventArgs(Value));
        }

        private class ValueChangedEventArgs : EventArgs
        {
            public ValueChangedEventArgs(TValue value)
            {
                Value = value;
            }

            public TValue Value { get; }
        }
    }
}