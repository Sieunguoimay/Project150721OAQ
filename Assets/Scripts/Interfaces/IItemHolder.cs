using System.Collections.Generic;

namespace Interfaces
{
    public interface IItemHolder<T>
    {
        void Attach(T item);
        void Detach(T item);
        IEnumerable<T> GetItems();
    }
}