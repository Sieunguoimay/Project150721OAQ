using UnityEngine;

namespace SNM
{
    public class Utils
    {
        public static T NewGameObject<T>() where T : MonoBehaviour
        {
            //create a GameObject that should be automatically added to the game scene
            var go = (new GameObject());
            var t = go.AddComponent<T>();
            go.name = t.GetType().Name;
            return t;
        }
    }

    public class Math
    {
        public static Vector3 Projection(Vector3 v, Vector3 up)
        {
            var n = Vector3.Cross(v, up);
            var tangent = Vector3.Cross(n, up);
            return Vector3.Dot(v, tangent) * tangent;
        }

        public static float CalculateJumpInitialVelocity(float s, float a, float t)
        {
            return (s - 0.5f * a * t * t) / t;
        }

        public static Vector3 MotionEquation(Vector3 initialPos, Vector3 initialVel, Vector3 initialAcc, float t)
        {
            return initialPos + initialVel * t + initialAcc * (0.5f * t * t);
        }
    }
}