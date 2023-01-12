using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Misc
{
    /// <summary>
    /// Specify external condition whether an object is in pool. It might be a state of the object i.e ParticleSystem.isPlaying
    /// </summary>
    /// <typeparam name="TPoolingItem"></typeparam>
    public class CustomConditionPooling<TPoolingItem> 
    {
        private readonly Func<TPoolingItem, bool> _inPoolCondition;
        private readonly Func<TPoolingItem> _actionOnCreate;
        [field: System.NonSerialized] public List<TPoolingItem> Items { get; }

        public CustomConditionPooling(Func<TPoolingItem, bool> inPoolCondition, Func<TPoolingItem> actionOnCreate)
        {
            _inPoolCondition = inPoolCondition;
            _actionOnCreate = actionOnCreate;
            Items = new List<TPoolingItem>();
        }

        public TPoolingItem GetFromPool()
        {
            var freePs = Items.FirstOrDefault(p => _inPoolCondition.Invoke(p));
            if (freePs!=null)
            {
                return freePs;
            }

            var newItem = _actionOnCreate.Invoke();
            Items.Add(newItem);
            return newItem;
        }
    }
}