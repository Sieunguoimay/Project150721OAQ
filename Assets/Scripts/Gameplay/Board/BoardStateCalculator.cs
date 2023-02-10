using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Board
{
    public abstract class BoardStateCalculator
    {
        public class StepData
        {
            public int state;
            public int tileIndex;
            public int data;

            public StepData(int state, int tileIndex, int data)
            {
                this.state = state;
                this.tileIndex = tileIndex;
                this.data = data;
            }
        }

        public static IEnumerator<StepData> Calculate(int[] newState, int tileIndex, bool direction)
        {
            var totalEaten = 0;
            var halfSize = newState.Length / 2;
            if (tileIndex % halfSize == 0)
            {
                //Mandarin tile
                Debug.LogError("Trying to start the move from an mandarin tile");
                yield return new StepData(-1, -1, 0);
            }

            var currentTileIndex = tileIndex;
            while (true)
            {
                var takenPieces = newState[currentTileIndex];
                newState[currentTileIndex] = 0;
                yield return new StepData(0, currentTileIndex, 0);

                for (var i = 0; i < takenPieces; i++)
                {
                    currentTileIndex = BoardTraveller.MoveNext(currentTileIndex, newState.Length, direction);
                    newState[currentTileIndex]++;
                    yield return new StepData(1, currentTileIndex, takenPieces - i - 1);
                }

                var nextTileIndex = BoardTraveller.MoveNext(currentTileIndex, newState.Length, direction);
                var nextTileIndex2 = BoardTraveller.MoveNext(nextTileIndex, newState.Length, direction);

                // Is next tile mandarin or no more citizens left to eat
                if (nextTileIndex % halfSize == 0 ||
                    newState[nextTileIndex] == 0 && newState[nextTileIndex2] == 0)
                {
                    yield return new StepData(-1, currentTileIndex, 0);
                    break;
                }

                if (newState[nextTileIndex] == 0 && nextTileIndex % halfSize != 0 &&
                    newState[nextTileIndex2] > 0)
                {
                    while (newState[nextTileIndex] == 0 && nextTileIndex % halfSize != 0 &&
                           newState[nextTileIndex2] > 0)
                    {
                        newState[nextTileIndex] = -1;
                        yield return new StepData(2, nextTileIndex, 0);
                        newState[nextTileIndex] = 0;

                        var count = newState[nextTileIndex2];
                        totalEaten += count;
                        newState[nextTileIndex2] = 0;

                        nextTileIndex = BoardTraveller.MoveNext(nextTileIndex2, newState.Length, direction);
                        nextTileIndex2 = BoardTraveller.MoveNext(nextTileIndex, newState.Length, direction);

                        yield return new StepData(3, currentTileIndex, count);
                    }

                    break;
                }

                if (newState[nextTileIndex] > 0 && nextTileIndex % halfSize != 0)
                {
                    // yield return new StepData(4, currentTileIndex, 0);
                    currentTileIndex = nextTileIndex;
                    continue;
                }

                yield return new StepData(-1, currentTileIndex, 0);

                break;
            }
        }
    }
}