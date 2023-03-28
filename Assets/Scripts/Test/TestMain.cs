using System;
using System.Collections.Generic;
using System.Linq;
using Common.UnityExtend.Reflection;
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
        // [SerializeField] private int testIndex;
        // [SerializeField] private bool testDirection;
        // [SerializeField] private bool reset;
        //
        // [ContextMenu("TestHorseGame")]
        // public void TestHorseGame()
        // {
        //     if (reset)
        //     {
        //         _horses = DefaultHorses.ToArray();
        //         _cells = DefaultCells.ToArray();
        //     }
        //
        //     var steps = Move(testIndex, testDirection);
        //     while (steps.MoveNext())
        //     {
        //         var cells = "";
        //         for (var i = 0; i < _cells.Length; i++)
        //         {
        //             var horse = "";
        //             for (var j = 0; j < _cells[i].Length; j++)
        //             {
        //                 if (_cells[i][j] == -1)
        //                 {
        //                     break;
        //                 }
        //
        //                 horse += $"({_horses[_cells[i][j]]})";
        //             }
        //
        //             var travellingHorse = steps.Current.State == 1 && steps.Current.TileIndex == i
        //                 ? $"({_horses[steps.Current.Data]})+"
        //                 : "";
        //             var eatable = steps.Current.State == 2 && steps.Current.TileIndex == i ? "X" : "";
        //             var eating = steps.Current.State is 3 or 0 && steps.Current.TileIndex == i
        //                 ? "+"
        //                 : "";
        //             var isMandarin = i % (_cells.Length / 2) == 0;
        //             cells += $" {(isMandarin?"[":"")}{eating}{horse}{travellingHorse}{eatable}{eating}{(isMandarin?"]":"")} |";
        //         }
        //
        //         var eat = steps.Current.State == 3 ? $"->{steps.Current.Data}" : "";
        //
        //         Debug.Log($"{cells}{eat}");
        //     }
        // }
        //
        // private IEnumerator<StepData> Move(int cellIndex, bool direction)
        // {
        //     var halfSize = _cells.Length / 2;
        //     var currentCellIndex = cellIndex;
        //
        //
        //     while (true)
        //     {
        //         var currentCell = _cells[currentCellIndex];
        //         var horseIndex = currentCell[0];
        //
        //         if (horseIndex == -1)
        //         {
        //             yield return new StepData(-1, currentCellIndex, 0);
        //             break;
        //         }
        //
        //         var steps = _horses[horseIndex];
        //         if (steps == 0)
        //         {
        //             yield return new StepData(-1, currentCellIndex, 0);
        //             break;
        //         }
        //
        //         yield return new StepData(0, currentCellIndex, horseIndex);
        //
        //         for (var i = 0; i < _cells[currentCellIndex].Length - 1; i++)
        //         {
        //             _cells[currentCellIndex][i] = _cells[currentCellIndex][i + 1];
        //             if (_cells[currentCellIndex][i + 1] == -1) break;
        //         }
        //
        //         for (var i = 0; i < steps; i++)
        //         {
        //             currentCellIndex = BoardTraveller.MoveNext(currentCellIndex, _cells.Length, direction);
        //             _horses[horseIndex]--;
        //             if (_cells[currentCellIndex][0] != -1)
        //             {
        //                 _horses[_cells[currentCellIndex][0]]++;
        //             }
        //
        //             yield return new StepData(1, currentCellIndex, horseIndex);
        //         }
        //
        //         for (var i = 0; i < _cells[currentCellIndex].Length - 1; i++)
        //         {
        //             if (_cells[currentCellIndex][i] != -1) continue;
        //             _cells[currentCellIndex][i] = horseIndex;
        //             break;
        //         }
        //
        //         var nextCellIndex = BoardTraveller.MoveNext(currentCellIndex, _cells.Length, direction);
        //         var nextCellIndex2 = BoardTraveller.MoveNext(nextCellIndex, _cells.Length, direction);
        //         if (_cells[nextCellIndex][0] >= 0 && _horses[_cells[nextCellIndex][0]] == 0
        //             || nextCellIndex % halfSize == 0
        //             || _cells[nextCellIndex][0] == -1 && _cells[nextCellIndex2][0] == -1
        //             || _cells[nextCellIndex][0] == -1 && _cells[nextCellIndex2][0] != -1 &&
        //             _horses[_cells[nextCellIndex2][0]] == 0
        //         )
        //         {
        //             //Stop
        //             yield return new StepData(-1, nextCellIndex, 0);
        //             break;
        //         }
        //
        //         if (_cells[nextCellIndex][0] == -1 && _cells[nextCellIndex2][0] != -1 &&
        //             _horses[_cells[nextCellIndex2][0]] > 0)
        //         {
        //             //Eat
        //             while (_cells[nextCellIndex][0] == -1 && _cells[nextCellIndex2][0] != -1 &&
        //                    _horses[_cells[nextCellIndex2][0]] > 0)
        //             {
        //                 yield return new StepData(2, nextCellIndex, 0);
        //
        //                 var count = _horses[_cells[nextCellIndex2][0]];
        //                 _horses[_cells[nextCellIndex2][0]] = 0;
        //
        //                 yield return new StepData(3, nextCellIndex2, count);
        //
        //                 nextCellIndex = BoardTraveller.MoveNext(nextCellIndex2, _cells.Length, direction);
        //                 nextCellIndex2 = BoardTraveller.MoveNext(nextCellIndex, _cells.Length, direction);
        //             }
        //
        //             break;
        //         }
        //
        //         if (_cells[nextCellIndex][0] >= 0 && _horses[_cells[nextCellIndex][0]] > 0)
        //         {
        //             //Continue;
        //             currentCellIndex = nextCellIndex;
        //         }
        //     }
        // }
        //
        // private static int[] DefaultHorses => new[]
        //     {5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5};
        //
        // private static int[][] DefaultCells => new[]
        // {
        //     new[] {0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,},
        //     new[] {1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,},
        //     new[] {2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,},
        //     new[] {3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,},
        //     new[] {4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,},
        //     new[] {5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,},
        //     new[] {6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,},
        //     new[] {7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,},
        //     new[] {8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,},
        //     new[] {9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,},
        //     new[] {10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        //     new[] {11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        // };
        //
        // private int[] _horses = DefaultHorses.ToArray();
        // private int[][] _cells = DefaultCells.ToArray();
        [ContextMenu("Test")]
        private void Test()
        {
            var a = new B();
            // Debug.Log(a.GetType());
            a.Test();
        }

        private class A
        {
            public virtual void Test()
            {
                Debug.Log(GetType());
                Debug.Log(this);
            }
        }

        private class B:A
        {
            
        }
    }
}