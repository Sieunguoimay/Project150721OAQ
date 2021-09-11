using UnityEngine;

namespace SNM
{
    public class BezierEasing : IEasing
    {
        float SUBDIVISION_PRECISION = 0.0000001f;
        float SUBDIVISION_MAX_ITERATIONS = 10f;
        int NEWTON_ITERATIONS = 4;
        float NEWTON_MIN_SLOPE = 0.001f;

        int kSplineTableSize = 11;
        float kSampleStepSize => 1f / (kSplineTableSize - 1f);

        private float[] sampleValues;
        private Vector2 p0;
        private Vector2 p1;

        public BezierEasing(Vector2 p0, Vector2 p1)
        {
            this.p0 = p0;
            this.p1 = p1;
            sampleValues = new float[kSplineTableSize];
            for (var i = 0; i < kSplineTableSize; ++i)
            {
                sampleValues[i] = calcBezier(i * kSampleStepSize, p0.x, p1.x);
            }
        }

        public float GetEase(float x)
        {
            if (x == 0f || System.Math.Abs(x - 1f) < 0.0001f) return x;

            return calcBezier(GetTForX(x), p0.y, p1.y);
        }

        private float A(float aA1, float aA2)
        {
            return 1f - 3f * aA2 + 3f * aA1;
        }

        private float B(float aA1, float aA2)
        {
            return 3f * aA2 - 6f * aA1;
        }

        private float C(float aA1)
        {
            return 3f * aA1;
        }

        private float getSlope(float aT, float aA1, float aA2)
        {
            return 3f * A(aA1, aA2) * aT * aT + 2f * B(aA1, aA2) * aT + C(aA1);
        }

        private float newtonRaphsonIterate(float aX, float aGuessT, float mX1, float mX2)
        {
            for (var i = 0; i < NEWTON_ITERATIONS; ++i)
            {
                var currentSlope = getSlope(aGuessT, mX1, mX2);
                if (currentSlope == 0f)
                {
                    return aGuessT;
                }

                var currentX = calcBezier(aGuessT, mX1, mX2) - aX;
                aGuessT -= currentX / currentSlope;
            }

            return aGuessT;
        }

        private float binarySubdivide(float aX, float aA, float aB, float mX1, float mX2)
        {
            float currentX, currentT = 0f;
            int i = 0;
            do
            {
                currentT = aA + (aB - aA) / 2f;
                currentX = calcBezier(currentT, mX1, mX2) - aX;
                if (currentX > 0.0)
                {
                    aB = currentT;
                }
                else
                {
                    aA = currentT;
                }
            } while (Mathf.Abs(currentX) > SUBDIVISION_PRECISION && ++i < SUBDIVISION_MAX_ITERATIONS);

            return currentT;
        }

        private float GetTForX(float f)
        {
            var intervalStart = 0f;
            var currentSample = 1;
            var lastSample = kSplineTableSize - 1;

            for (; currentSample != lastSample && sampleValues[currentSample] <= f; ++currentSample)
            {
                intervalStart += kSampleStepSize;
            }

            --currentSample;

            // Interpolate to provide an initial guess for t
            var dist = (f - sampleValues[currentSample]) /
                       (sampleValues[currentSample + 1] - sampleValues[currentSample]);
            var guessForT = intervalStart + dist * kSampleStepSize;


            var initialSlope = getSlope(guessForT, p0.x, p1.x);
            if (initialSlope >= NEWTON_MIN_SLOPE)
            {
                return newtonRaphsonIterate(f, guessForT, p0.x, p1.x);
            }
            else if (initialSlope == 0f)
            {
                return guessForT;
            }
            else
            {
                return binarySubdivide(f, intervalStart, intervalStart + kSampleStepSize, p0.x, p1.x);
            }
        }

        private float calcBezier(float aT, float aA1, float aA2)
        {
            return ((A(aA1, aA2) * aT + B(aA1, aA2)) * aT + C(aA1)) * aT;
        }

        private static BezierEasing blueprint1;

        public static BezierEasing Blueprint1 =>
            blueprint1 ?? (blueprint1 = new BezierEasing(new Vector2(0, 0.23f), new Vector2(1f, 0.77f)));
    }
}