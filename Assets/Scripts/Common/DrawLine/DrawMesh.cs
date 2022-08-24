using UnityEngine;
using Vector3 = System.Numerics.Vector3;

namespace Common.DrawLine
{
    public class DrawMesh
    {
        public Mesh Mesh { get; }
        
        public DrawMesh()
        {
            Mesh = new Mesh();
        }
        public void Draw(Vector3 point)
        {
            
        }
    }
}