using System.Collections.Generic;
using UnityEngine;

namespace InGame.Common
{
    public class Path : MonoBehaviour
    {
        public List<Vector3> points;
        public float Radius;

        public Path()
        {
            Radius = 0.5f;
            points = new List<Vector3>();
        }

        public void AddPoint(Vector3 point)
        {
            points.Add(point);
        }
    }
}