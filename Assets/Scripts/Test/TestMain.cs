using System;
using System.Collections.Generic;
using Common.UnityExtend.Reflection;
using Gameplay.Board;
using InGame.Common;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace Test
{
    public class TestMain : MonoBehaviour
    {
        // [NonSerialized] private int _counter;
        //
        // [ContextMenu("Test")]
        // private void Test()
        // {
        //     var enumerator = GetIntegerEnumerator();
        //     MyStartCoroutine(enumerator);
        // }
        //
        // private void MyStartCoroutine(IEnumerator<int> enumerator)
        // {
        //     // while (enumerator.MoveNext())
        //     // {
        //     //     Debug.Log(enumerator.Current);
        //     // }
        //
        //     foreach (var a in this)
        //     {
        //         Debug.Log(a);
        //     }
        // }
        //
        // public IEnumerator<int> GetEnumerator()
        // {
        //     return GetIntegerEnumerator();
        // }
        //
        // public IEnumerator<int> GetIntegerEnumerator()
        // {
        //     Debug.Log("Hello kitty " + _counter++);
        //     yield return 1;
        //     Debug.Log("Hello kitty " + _counter++);
        //     yield return 2;
        //     Debug.Log("Hello kitty " + _counter++);
        //     yield return 3;
        //     Debug.Log("Hello kitty " + _counter++);
        //     yield return 5;
        //     Debug.Log("Hello kitty " + _counter++);
        //
        // }

        [ContextMenu("TestHorseGame")]
        public void TestHorseGame()
        {
        }

        private IEnumerable<StepData> Move(int cellIndex, bool direction)
        {
            var halfSize = _cells.Length / 2;
            var currentCellIndex = cellIndex;

            var currentCell = _cells[currentCellIndex];
            var horseIndex = currentCell[0];
            var steps = _horses[horseIndex];

            for (var i = 0; i < currentCell.Length - 1; i++)
            {
                currentCell[i] = currentCell[i + 1];
                if (currentCell[i + 1] == -1) break;
            }

            for (var i = 0; i < steps; i++)
            {
                currentCellIndex = BoardTraveller.MoveNext(currentCellIndex, _cells.Length, direction);
                _horses[horseIndex]--;
                if (_cells[currentCellIndex][0] != -1)
                {
                    _horses[_cells[currentCellIndex][0]]++;
                }
            }

            currentCellIndex = BoardTraveller.MoveNext(currentCellIndex, _cells.Length, direction);
            var currentCellIndex2 = BoardTraveller.MoveNext(currentCellIndex, _cells.Length, direction);
            if (_cells[currentCellIndex][0] >= 0 && _horses[_cells[currentCellIndex][0]] == 0
                || currentCellIndex % halfSize == 0
                || _cells[currentCellIndex][0] == -1 && _cells[currentCellIndex2][0] == -1
                || _cells[currentCellIndex][0] == -1 && _cells[currentCellIndex2][0] != -1 && _horses[_cells[currentCellIndex2][0]] == 0
            )
            {
                //Stop
            }

            if (_cells[currentCellIndex][0] == -1 && _cells[currentCellIndex2][0] != -1 && _horses[_cells[currentCellIndex2][0]] > 0)
            {
                //Eat
            }

            if (_cells[currentCellIndex][0] >= 0 && _horses[_cells[currentCellIndex][0]] > 0)
            {
                //Continue;
            }

            return null;  
        }

        private int[] _horses = {5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5};

        private int[][] _cells =
        {
            new[] {0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,},
            new[] {1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,},
            new[] {2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,},
            new[] {3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,},
            new[] {4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,},
            new[] {5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,},
            new[] {6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,},
            new[] {7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,},
            new[] {8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,},
            new[] {9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,},
            new[] {10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            new[] {11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        };
    }
}