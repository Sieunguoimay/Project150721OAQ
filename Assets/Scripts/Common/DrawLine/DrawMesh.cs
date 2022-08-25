using UnityEngine;

namespace Common.DrawLine
{
    public class DrawMesh
    {
        private Mesh _mesh;

        public Mesh CreateNew()
        {
            _mesh = new Mesh();
            
            var vertices = new Vector3[4];
            var uv = new Vector2[4];
            var triangles = new int[6];
            
            vertices[0] = new Vector3(-1f, 0f, 1f);
            vertices[1] = new Vector3(-1f, 0f, -1f);
            vertices[2] = new Vector3(1f, 0f, -1f);
            vertices[3] = new Vector3(1f, 0f, 1f);

            uv[0] = Vector2.zero;
            uv[1] = Vector2.zero;
            uv[2] = Vector2.zero;
            uv[3] = Vector2.zero;

            triangles[0] = 0;
            triangles[1] = 3;
            triangles[2] = 1;
            triangles[3] = 1;
            triangles[4] = 3;
            triangles[5] = 2;

            _mesh.vertices = vertices;
            _mesh.uv = uv;
            _mesh.triangles = triangles;
            
            return _mesh;
        }

        public void Draw(Vector2 point)
        {
        }
    }
}