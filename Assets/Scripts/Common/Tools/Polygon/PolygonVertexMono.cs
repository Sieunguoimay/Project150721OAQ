using UnityEngine;

namespace Common.Tools.Polygon
{
    public interface IPolygonVertex
    {
        Transform Transform { get; }
        int Index { get; set; }
    }

    public class PolygonVertexMono : MonoBehaviour, IPolygonVertex
    {
        public Transform Transform => transform;
        public int Index { get; set; }
    }
}