using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Common.Tools.Polygon
{
    public static class PolygonTriangulate
    {
        // private readonly Vector2[] _points;
        //
        // public PolygonTriangulate(Vector2[] points)
        // {
        //     _points = points;
        // }
        //
        // public PolygonTriangulate(Vector3[] points)
        // {
        //     _points = points.Select(vertex => new Vector2(vertex.x, vertex.z)).ToArray();
        // }

        // public static bool Triangulate(Vector3[] vertices, int[] indices, int indexOffset = 0, int vertexOffset = 0, int numVertices = 0)
        // {
        //     if (numVertices == 0)
        //         numVertices = vertices.Length;
        //
        //     if (numVertices < 3)
        //         return false;
        //
        //     var workingIndices = new int[numVertices];
        //     if (Area(vertices, vertexOffset, numVertices) > 0)
        //     {
        //         for (int v = 0; v < numVertices; v++)
        //             workingIndices[v] = v;
        //     }
        //     else
        //     {
        //         for (int v = 0; v < numVertices; v++)
        //             workingIndices[v] = (numVertices - 1) - v;
        //     }
        //
        //     int nv = numVertices;
        //     int count = 2 * nv;
        //     int currentIndex = indexOffset;
        //     for (int m = 0, v = nv - 1; nv > 2;)
        //     {
        //         if (count-- <= 0)
        //             return false;
        //
        //         int u = v;
        //         if (nv <= u)
        //             u = 0;
        //
        //         v = u + 1;
        //         if (nv <= v)
        //             v = 0;
        //
        //         int w = v + 1;
        //         if (nv <= w)
        //             w = 0;
        //
        //         if (Snip(vertices, u, v, w, nv, workingIndices))
        //         {
        //             indices[currentIndex++] = workingIndices[u];
        //             indices[currentIndex++] = workingIndices[v];
        //             indices[currentIndex++] = workingIndices[w];
        //             m++;
        //
        //             for (int s = v, t = v + 1; t < nv; s++, t++)
        //                 workingIndices[s] = workingIndices[t];
        //
        //             nv--;
        //             count = 2 * nv;
        //         }
        //     }
        //
        //     return true;
        // }
        //
        // public static float Area(Vector3[] vertices, int vertexOffset = 0, int numVertices = 0)
        // {
        //     if (numVertices == 0)
        //         numVertices = vertices.Length;
        //
        //     float area = 0.0f;
        //     for (int p = vertexOffset + numVertices - 1, q = 0; q < numVertices; p = q++)
        //         area += vertices[p].x * vertices[q].y - vertices[q].x * vertices[p].y;
        //
        //     return area * 0.5f;
        // }
        //
        // private static bool Snip(Vector3[] vertices, int u, int v, int w, int n, int[] workingIndices)
        // {
        //     Vector2 A = vertices[workingIndices[u]];
        //     Vector2 B = vertices[workingIndices[v]];
        //     Vector2 C = vertices[workingIndices[w]];
        //
        //     if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
        //         return false;
        //
        //     for (int p = 0; p < n; p++)
        //     {
        //         if ((p == u) || (p == v) || (p == w))
        //             continue;
        //
        //         Vector2 P = vertices[workingIndices[p]];
        //
        //         if (InsideTriangle(A, B, C, P))
        //             return false;
        //     }
        //
        //     return true;
        // }

        public static int[] Triangulate(Vector2[] points)
        {
            var indices = new List<int>();

            var n = points.Length;
            if (n < 3)
                return indices.ToArray();

            var V = new int[n];
            if (Area(points) > 0)
            {
                for (var v = 0; v < n; v++)
                    V[v] = v;
            }
            else
            {
                for (var v = 0; v < n; v++)
                    V[v] = (n - 1) - v;
            }

            var nv = n;
            var count = 2 * nv;
            for (var v = nv - 1; nv > 2;)
            {
                if (count-- <= 0)
                    return indices.ToArray();

                var u = v;
                if (nv <= u)
                    u = 0;

                v = u + 1;
                if (nv <= v)
                    v = 0;

                var w = v + 1;
                if (nv <= w)
                    w = 0;

                if (Snip(points, u, v, w, nv, V))
                {
                    var a = V[u];
                    var b = V[v];
                    var c = V[w];
                    indices.Add(a);
                    indices.Add(b);
                    indices.Add(c);

                    for (int s = v, t = v + 1; t < nv; s++, t++)
                        V[s] = V[t];

                    nv--;
                    count = 2 * nv;
                }
            }

            indices.Reverse();
            return indices.ToArray();
        }

        private static float Area(Vector2[] points)
        {
            var n = points.Length;
            var A = 0.0f;
            for (int p = n - 1, q = 0; q < n; p = q++)
            {
                var pval = points[p];
                var qval = points[q];
                A += pval.x * qval.y - qval.x * pval.y;
            }

            return A * 0.5f;
        }

        private static bool Snip(Vector2[] points, int u, int v, int w, int n, int[] V)
        {
            var A = points[V[u]];
            var B = points[V[v]];
            var C = points[V[w]];

            if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
                return false;

            for (int p = 0; p < n; p++)
            {
                if ((p == u) || (p == v) || (p == w))
                    continue;

                var P = points[V[p]];

                if (InsideTriangle(A, B, C, P))
                    return false;
            }

            return true;
        }

        private static bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
        {
            var ax = C.x - B.x;
            var ay = C.y - B.y;
            var bx = A.x - C.x;
            var by = A.y - C.y;
            var cx = B.x - A.x;
            var cy = B.y - A.y;
            var apx = P.x - A.x;
            var apy = P.y - A.y;
            var bpx = P.x - B.x;
            var bpy = P.y - B.y;
            var cpx = P.x - C.x;
            var cpy = P.y - C.y;

            var aCROSSbp = ax * bpy - ay * bpx;
            var cCROSSap = cx * apy - cy * apx;
            var bCROSScp = bx * cpy - by * cpx;

            return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
        }
    }
}