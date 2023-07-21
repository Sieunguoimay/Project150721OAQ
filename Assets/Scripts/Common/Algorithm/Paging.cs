using System.Collections.Generic;
using UnityEngine;

namespace Common.Algorithm
{
    public class Paging<TItem>
    {
        private readonly IReadOnlyList<TItem> _items;
        private readonly int _pageSize;
        private readonly int _pageNum;
        private int _pageIndex;

        public int PageSize => _pageSize;
        public int PageIndex => _pageIndex;
        public int ItemsCount => _items.Count;
        public int PageNum => _pageNum;

        public Paging(IReadOnlyList<TItem> items, int pageSize)
        {
            _items = items;
            _pageSize = Mathf.Max(1, pageSize);
            _pageNum = Mathf.FloorToInt(items.Count / pageSize);
            if (items.Count % pageSize != 0)
            {
                _pageNum++;
            }
        }

        public void NextPage()
        {
            _pageIndex = Mathf.Min(_pageIndex + 1, _pageNum - 1);
        }
        public void PrevPage()
        {
            _pageIndex = Mathf.Max(_pageIndex - 1, 0);
        }
        public IEnumerable<TItem> GetPageItems()
        {
            var from = _pageIndex * _pageSize;
            var n = GetCurrentPageSize();
            for (var i = from; i < n; i++)
            {
                yield return _items[i];
            }
        }
        private int GetCurrentPageSize()
        {
            return _pageIndex < _pageNum - 1 ? _pageSize : _items.Count - _pageIndex * _pageSize;
        }
    }
}
