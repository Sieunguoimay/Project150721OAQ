using System;
using System.Collections.Generic;
using Common.UnityExtend.Reflection;
using InGame.Common;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace Test
{
    public class TestMain : MonoBehaviour
    {
        [NonSerialized] private int _counter;

        [ContextMenu("Test")]
        private void Test()
        {
            var enumerator = GetIntegerEnumerator();
            MyStartCoroutine(enumerator);
        }

        private void MyStartCoroutine(IEnumerator<int> enumerator)
        {
            // while (enumerator.MoveNext())
            // {
            //     Debug.Log(enumerator.Current);
            // }

            foreach (var a in this)
            {
                Debug.Log(a);
            }
        }

        public IEnumerator<int> GetEnumerator()
        {
            return GetIntegerEnumerator();
        }

        public IEnumerator<int> GetIntegerEnumerator()
        {
            Debug.Log("Hello kitty " + _counter++);
            yield return 1;
            Debug.Log("Hello kitty " + _counter++);
            yield return 2;
            Debug.Log("Hello kitty " + _counter++);
            yield return 3;
            Debug.Log("Hello kitty " + _counter++);
            yield return 5;
            Debug.Log("Hello kitty " + _counter++);

        }
    }
}