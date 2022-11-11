using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Common.UnityExtend
{
    public class UnityObjectPooling<TPrefab> where TPrefab : UnityEngine.Object
    {
        private readonly TPrefab _psPrefab;
        private readonly List<TPrefab> _pool;

        private readonly Transform _container;
        private readonly Func<TPrefab, bool> _availableCondition;

        public UnityObjectPooling(Transform container, TPrefab psPrefab, Func<TPrefab, bool> availableCondition)
        {
            _container = container;
            _psPrefab = psPrefab;
            _availableCondition = availableCondition;
            _pool = new List<TPrefab>();
        }

        public TPrefab GetFromPool()
        {
            while (true)
            {
                var freePs = _pool.FirstOrDefault(p => _availableCondition.Invoke(p));
                if (freePs)
                {
                    return freePs;
                }

                _pool.Add(Object.Instantiate(_psPrefab, _container));
            }
        }
    }

}