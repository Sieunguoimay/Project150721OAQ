using System.Collections;
using System.Collections.Generic;

namespace Gameplay.OptionSystem
{
    public class DynamicOptionItem : OptionItem
    {
        private readonly OptionQueue _optionQueue;

        public DynamicOptionItem(OptionQueue optionQueue)
        {
            _optionQueue = optionQueue;
        }

        public override void ApplySelectedValue(OptionValue selectedValue)
        {
            base.ApplySelectedValue(selectedValue);
            var index = _optionQueue.Options.IndexOf(this) + 1;
            if (SelectedValue is OptionItemArrayOptionValue sv)
            {
                var items = sv.Value;
                foreach (var item in items)
                {
                    _optionQueue.Options.Insert(index++, item);
                }
            }
        }
    }

    public class OptionItemArrayOptionValue : OptionValue
    {
        public OptionItemArrayOptionValue(IEnumerable items) : base(items)
        {
        }

        public IEnumerable<OptionItem> Value => DynamicValue as OptionItem[];
    }
}