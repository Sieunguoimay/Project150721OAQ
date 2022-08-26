using System;
using System.Collections.Generic;
using UnityEngine;

namespace Common.Algorithm
{
    public static class AntColonyOptimization
    {
        private static readonly System.Random Random = new(0);

        // influence of pheromone on direction
        private const int alpha = 3;

        // influence of adjacent node distance
        private const int beta = 2;

        // pheromone decrease factor
        private const double rho = 0.01;

        // pheromone increase factor
        private const double Q = 2.0;

        public static int[] Solve(int[][] weightMatrix, int numCities, int numAnts = 4, int maxTime = 1000, bool randomize = true)
        {
            var ants = InitAnts(numAnts, numCities, randomize); // initialize ants to random trails

            // ShowAnts(ants, weightMatrix);

            var bestTrail = BestTrail(ants, weightMatrix); // determine the best initial trail
            var bestLength = Length(bestTrail, weightMatrix); // the length of the best trail
            var pheromones = InitPheromones(numCities);

            var time = 0;
            while (time < maxTime)
            {
                UpdateAnts(ants, pheromones, weightMatrix);
                UpdatePheromones(pheromones, ants, weightMatrix);

                var currBestTrail = BestTrail(ants, weightMatrix);
                var currBestLength = Length(currBestTrail, weightMatrix);
                if (currBestLength < bestLength)
                {
                    bestLength = currBestLength;
                    bestTrail = currBestTrail;
                    // Debug.Log("New best length of " + bestLength.ToString("F1") + " found at time " + time);
                }

                time += 1;
            }

            return bestTrail;
        }

        private static int[][] InitAnts(int numAnts, int numCities, bool randomize)
        {
            var ants = new int[numAnts][];
            for (var k = 0; k <= numAnts - 1; k++)
            {
                if (randomize)
                {
                    var start = Random.Next(0, numCities);
                    ants[k] = RandomTrail(start, numCities);
                }
                else
                {
                    ants[k] = new int[numCities];
                    for (var i = 0; i <= numCities - 1; i++)
                    {
                        ants[k][i] = i;
                    }
                }
            }

            return ants;
        }

        private static int[] RandomTrail(int start, int numCities)
        {
            // helper for InitAnts
            var trail = new int[numCities];

            // sequential
            for (var i = 0; i <= numCities - 1; i++)
            {
                trail[i] = i;
            }

            // Fisher-Yates shuffle
            for (var i = 0; i <= numCities - 1; i++)
            {
                var r = Random.Next(i, numCities);
                var tmp = trail[r];
                trail[r] = trail[i];
                trail[i] = tmp;
            }

            var idx = IndexOfTarget(trail, start);
            // put start at [0]
            var temp = trail[0];
            trail[0] = trail[idx];
            trail[idx] = temp;

            return trail;
        }

        private static int IndexOfTarget(IReadOnlyList<int> trail, int target)
        {
            // helper for RandomTrail
            for (var i = 0; i <= trail.Count - 1; i++)
            {
                if (trail[i] == target)
                {
                    return i;
                }
            }

            throw new Exception("Target not found in IndexOfTarget");
        }

        private static double Length(IReadOnlyList<int> trail, int[][] weights)
        {
            // total length of a trail
            var result = 0.0;
            for (var i = 0; i <= trail.Count - 2; i++)
            {
                result += Distance(trail[i], trail[i + 1], weights);
            }

            return result;
        }

        // -------------------------------------------------------------------------------------------- 

        private static int[] BestTrail(IReadOnlyList<int[]> ants, int[][] weights)
        {
            // best trail has shortest total length
            var bestLength = Length(ants[0], weights);
            var idxBestLength = 0;
            for (var k = 1; k <= ants.Count - 1; k++)
            {
                var len = Length(ants[k], weights);
                if (len < bestLength)
                {
                    bestLength = len;
                    idxBestLength = k;
                }
            }

            var numCities = ants[0].Length;
            //INSTANT VB NOTE: The local variable bestTrail was renamed since Visual Basic will not allow local variables with the same name as their enclosing function or property:
            var bestTrailRenamed = new int[numCities];
            ants[idxBestLength].CopyTo(bestTrailRenamed, 0);
            return bestTrailRenamed;
        }

        // --------------------------------------------------------------------------------------------

        private static double[][] InitPheromones(int numCities)
        {
            var pheromones = new double[numCities][];
            for (var i = 0; i <= numCities - 1; i++)
            {
                pheromones[i] = new double[numCities];
            }

            for (var i = 0; i <= pheromones.Length - 1; i++)
            {
                for (var j = 0; j <= pheromones[i].Length - 1; j++)
                {
                    pheromones[i][j] = 0.01;
                    // otherwise first call to UpdateAnts -> BuiuldTrail -> NextNode -> MoveProbs => all 0.0 => throws
                }
            }

            return pheromones;
        }

        // --------------------------------------------------------------------------------------------

        private static void UpdateAnts(int[][] ants, double[][] pheromones, int[][] weights)
        {
            var numCities = pheromones.Length;
            for (var k = 0; k <= ants.Length - 1; k++)
            {
                var start = Random.Next(0, numCities);
                var newTrail = BuildTrail(k, start, pheromones, weights);
                ants[k] = newTrail;
            }
        }

        private static int[] BuildTrail(int k, int start, double[][] pheromones, int[][] weights)
        {
            var numCities = pheromones.Length;
            var trail = new int[numCities];
            var visited = new bool[numCities];
            trail[0] = start;
            visited[start] = true;
            for (var i = 0; i <= numCities - 2; i++)
            {
                var cityX = trail[i];
                var next = NextCity(k, cityX, visited, pheromones, weights);
                trail[i + 1] = next;
                visited[next] = true;
            }

            return trail;
        }

        private static int NextCity(int k, int cityX, bool[] visited, double[][] pheromones, int[][] weights)
        {
            // for ant k (with visited[]), at nodeX, what is next node in trail?
            var probs = MoveProbs(k, cityX, visited, pheromones, weights);

            var cumul = new double[probs.Length + 1];
            for (var i = 0; i <= probs.Length - 1; i++)
            {
                cumul[i + 1] = cumul[i] + probs[i];
                // consider setting cumul[cuml.Length-1] to 1.00
            }

            double p = Random.NextDouble();

            for (int i = 0; i <= cumul.Length - 2; i++)
            {
                if (p >= cumul[i] && p < cumul[i + 1])
                {
                    return i;
                }
            }

            throw new Exception("Failure to return valid city in NextCity");
        }

        private static double[] MoveProbs(int k, int cityX, bool[] visited, double[][] pheromones, int[][] dists)
        {
            // for ant k, located at nodeX, with visited[], return the prob of moving to each city
            var numCities = pheromones.Length;
            var taueta = new double[numCities];
            // inclues cityX and visited cities
            var sum = 0.0;
            // sum of all tauetas
            // i is the adjacent city
            for (var i = 0; i <= taueta.Length - 1; i++)
            {
                if (i == cityX)
                {
                    taueta[i] = 0.0;
                    // prob of moving to self is 0
                }
                else if (visited[i] == true)
                {
                    taueta[i] = 0.0;
                    // prob of moving to a visited city is 0
                }
                else
                {
                    taueta[i] = Math.Pow(pheromones[cityX][i], alpha) * Math.Pow((1.0 / Distance(cityX, i, dists)), beta);
                    // could be huge when pheromone[][] is big
                    if (taueta[i] < 0.0001)
                    {
                        taueta[i] = 0.0001;
                    }
                    else if (taueta[i] > (double.MaxValue / (numCities * 100)))
                    {
                        taueta[i] = double.MaxValue / (numCities * 100);
                    }
                }

                sum += taueta[i];
            }

            var probs = new double[numCities];
            for (var i = 0; i <= probs.Length - 1; i++)
            {
                probs[i] = taueta[i] / sum;
                // big trouble if sum = 0.0
            }

            return probs;
        }

        // --------------------------------------------------------------------------------------------

        private static void UpdatePheromones(double[][] pheromones, int[][] ants, int[][] dists)
        {
            for (int i = 0; i <= pheromones.Length - 1; i++)
            {
                for (int j = i + 1; j <= pheromones[i].Length - 1; j++)
                {
                    for (int k = 0; k <= ants.Length - 1; k++)
                    {
                        double length = AntColonyOptimization.Length(ants[k], dists);
                        // length of ant k trail
                        double decrease = (1.0 - rho) * pheromones[i][j];
                        double increase = 0.0;
                        if (EdgeInTrail(i, j, ants[k]) == true)
                        {
                            increase = (Q / length);
                        }

                        pheromones[i][j] = decrease + increase;

                        if (pheromones[i][j] < 0.0001)
                        {
                            pheromones[i][j] = 0.0001;
                        }
                        else if (pheromones[i][j] > 100000.0)
                        {
                            pheromones[i][j] = 100000.0;
                        }

                        pheromones[j][i] = pheromones[i][j];
                    }
                }
            }
        }

        private static bool EdgeInTrail(int cityX, int cityY, int[] trail)
        {
            // are cityX and cityY adjacent to each other in trail[]?
            var lastIndex = trail.Length - 1;
            var idx = IndexOfTarget(trail, cityX);

            if (idx == 0 && trail[1] == cityY)
            {
                return true;
            }
            else if (idx == 0 && trail[lastIndex] == cityY)
            {
                return true;
            }
            else if (idx == 0)
            {
                return false;
            }
            else if (idx == lastIndex && trail[lastIndex - 1] == cityY)
            {
                return true;
            }
            else if (idx == lastIndex && trail[0] == cityY)
            {
                return true;
            }
            else if (idx == lastIndex)
            {
                return false;
            }
            else if (trail[idx - 1] == cityY)
            {
                return true;
            }
            else if (trail[idx + 1] == cityY)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        // --------------------------------------------------------------------------------------------

        private static int[][] MakeGraphDistances(int numCities)
        {
            int[][] dists = new int[numCities][];
            for (int i = 0; i <= dists.Length - 1; i++)
            {
                dists[i] = new int[numCities];
            }

            for (int i = 0; i <= numCities - 1; i++)
            {
                for (int j = i + 1; j <= numCities - 1; j++)
                {
                    int d = Random.Next(1, 9);
                    // [1,8]
                    dists[i][j] = d;
                    dists[j][i] = d;
                }
            }

            return dists;
        }

        private static double Distance(int cityX, int cityY, int[][] dists)
        {
            return dists[cityX][cityY];
        }

        // --------------------------------------------------------------------------------------------

        private static void Display(IReadOnlyList<int> trail)
        {
            var str = "";
            for (var i = 0; i <= trail.Count - 1; i++)
            {
                str += trail[i] + " ";
                if (i > 0 && i % 20 == 0)
                {
                    str += "\n";
                }
            }

            str += "\n";
            Debug.Log(str);
        }


        private static void ShowAnts(IReadOnlyList<int[]> ants, int[][] weightMatrix)
        {
            for (var i = 0; i <= ants.Count - 1; i++)
            {
                var str = i + ": [ ";

                for (var j = 0; j <= 3; j++)
                {
                    str += ants[i][j] + " ";
                }

                str += ". . . ";

                for (var j = ants[i].Length - 4; j <= ants[i].Length - 1; j++)
                {
                    str += ants[i][j] + " ";
                }

                str += "] len = ";
                var len = Length(ants[i], weightMatrix);
                str += len.ToString("F1");
                Debug.Log(str);
            }
        }

        private static void Display(IReadOnlyList<double[]> pheromones)
        {
            for (var i = 0; i <= pheromones.Count - 1; i++)
            {
                var str = i + ": ";

                for (var j = 0; j <= pheromones[i].Length - 1; j++)
                {
                    str += pheromones[i][j].ToString("F4").PadLeft(8) + " ";
                }

                Debug.Log(str);
            }
        }

        public static void Test()
        {
            try
            {
                const int numCities = 60;
                const int numAnts = 4;
                const int maxTime = 1000;

                var weightMatrix = MakeGraphDistances(numCities);

                var bestTrail = Solve(weightMatrix, numCities, numAnts, maxTime);
                Display(bestTrail);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
        }
    }
    // class AntColonyProgram
}