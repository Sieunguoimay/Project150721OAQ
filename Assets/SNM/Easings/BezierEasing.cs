using UnityEngine;

namespace SNM.Easings
{
    public interface IEasing
    {
        float GetEase(float x);
    }

    public class BezierEasing : IEasing
    {
        private const float SubdivisionPrecision = 0.0000001f;
        private const float SubdivisionMAXIterations = 10f;
        private const int NewtonIterations = 4;
        private const float NewtonMINSlope = 0.001f;

        private const int KSplineTableSize = 11;
        private static float KSampleStepSize => 1f / (KSplineTableSize - 1f);

        private readonly float[] _sampleValues;
        private readonly Vector2 _p0;
        private readonly Vector2 _p1;

        public BezierEasing(Vector2 p0, Vector2 p1)
        {
            _p0 = p0;
            _p1 = p1;
            _sampleValues = new float[KSplineTableSize];
            for (var i = 0; i < KSplineTableSize; ++i)
            {
                _sampleValues[i] = CalcBezier(i * KSampleStepSize, p0.x, p1.x);
            }
        }

        public float GetEase(float x)
        {
            if (x == 0f || System.Math.Abs(x - 1f) < 0.0001f) return x;

            return CalcBezier(GetTForX(x), _p0.y, _p1.y);
        }

        private static float A(float aA1, float aA2)
        {
            return 1f - 3f * aA2 + 3f * aA1;
        }

        private static float B(float aA1, float aA2)
        {
            return 3f * aA2 - 6f * aA1;
        }

        private static float C(float aA1)
        {
            return 3f * aA1;
        }

        private static float GetSlope(float aT, float aA1, float aA2)
        {
            return 3f * A(aA1, aA2) * aT * aT + 2f * B(aA1, aA2) * aT + C(aA1);
        }

        private float NewtonRaphsonIterate(float aX, float aGuessT, float mX1, float mX2)
        {
            for (var i = 0; i < NewtonIterations; ++i)
            {
                var currentSlope = GetSlope(aGuessT, mX1, mX2);
                if (currentSlope == 0f)
                {
                    return aGuessT;
                }

                var currentX = CalcBezier(aGuessT, mX1, mX2) - aX;
                aGuessT -= currentX / currentSlope;
            }

            return aGuessT;
        }

        private float BinarySubdivide(float aX, float aA, float aB, float mX1, float mX2)
        {
            float currentX, currentT = 0f;
            var i = 0;
            do
            {
                currentT = aA + (aB - aA) / 2f;
                currentX = CalcBezier(currentT, mX1, mX2) - aX;
                if (currentX > 0.0)
                {
                    aB = currentT;
                }
                else
                {
                    aA = currentT;
                }
            } while (Mathf.Abs(currentX) > SubdivisionPrecision && ++i < SubdivisionMAXIterations);

            return currentT;
        }

        private float GetTForX(float f)
        {
            var intervalStart = 0f;
            var currentSample = 1;
            const int lastSample = KSplineTableSize - 1;

            for (; currentSample != lastSample && _sampleValues[currentSample] <= f; ++currentSample)
            {
                intervalStart += KSampleStepSize;
            }

            --currentSample;

            // Interpolate to provide an initial guess for t
            var dist = (f - _sampleValues[currentSample]) /
                       (_sampleValues[currentSample + 1] - _sampleValues[currentSample]);
            var guessForT = intervalStart + dist * KSampleStepSize;


            var initialSlope = GetSlope(guessForT, _p0.x, _p1.x);
            return initialSlope switch
            {
                >= NewtonMINSlope => NewtonRaphsonIterate(f, guessForT, _p0.x, _p1.x),
                0f => guessForT,
                _ => BinarySubdivide(f, intervalStart, intervalStart + KSampleStepSize, _p0.x, _p1.x)
            };
        }

        private float CalcBezier(float aT, float aA1, float aA2)
        {
            return ((A(aA1, aA2) * aT + B(aA1, aA2)) * aT + C(aA1)) * aT;
        }

        public static BezierEasing CreateBezierEasing(float a, float b) => new(new Vector2(0, a), new Vector2(1f, b));
    }
}