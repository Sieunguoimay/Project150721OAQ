using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Common.Tools.Polygon
{
    public class PolygonMeshGeneratorMono : MonoBehaviour
    {
        [SerializeField] private PolygonVertexGroupMono[] vertexGroups;
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private string savePath = "Assets/GeneratedMesh/";
        [SerializeField] private string meshName = "Untitled";
        [SerializeField] private bool noFaces = false;

        private Mesh _mesh;

        //When the array of vertices changed, call this
        [ContextMenu("ValidatePoints")]
        public void ValidatePoints()
        {
            var vertices = GetComponentsInChildren<IPolygonVertexGroup>();
            vertexGroups = vertices.Select(v => v as PolygonVertexGroupMono).ToArray();
            var index = 0;
            foreach (var p in vertices)
            {
                p.Index = index++;
                p.Transform.gameObject.name = $"VertexGroup - {p.Index}";
                p.ValidatePoints();
            }
        }

        [ContextMenu(nameof(CreateMeshInMemory))]
        public void CreateMeshInMemory()
        {
            IEnumerable<Vector3> vertices = new List<Vector3>();
            var triangles = new List<int>();
            var offset = 0;
            foreach (var vg in vertexGroups)
            {
                var points = vg.Points;

                if (points.Length < 3) return;
                meshFilter.mesh = _mesh = new UnityEngine.Mesh();
                _mesh.name = meshName;

                var ps = points
                    .Select(t => transform.InverseTransformPoint(vg.Transform.TransformPoint(t.localPosition)))
                    .ToList();
                if (!noFaces)
                {
                    var tris = PolygonTriangulate.Triangulate(
                        ps.Select(vertex => new Vector2(vertex.x, vertex.z)).ToArray());

                    for (var i = 0; i < tris.Length; i++)
                    {
                        triangles.Add(offset + tris[i]);
                    }
                }

                vertices = vertices.Concat(ps);
                offset += ps.Count;
            }

            _mesh.vertices = vertices.ToArray();
            if (!noFaces)
            {
                _mesh.triangles = triangles.ToArray();
            }
        }

#if UNITY_EDITOR
        [ContextMenu(nameof(SaveMeshToAsset))]
        public void SaveMeshToAsset()
        {
            if (meshFilter.sharedMesh == null || !vertexGroups.Any(vg => vg.Points.Length >= 3)) return;

            var path = $"{savePath}/{_mesh.name}";

            AssetDatabase.CreateAsset(meshFilter.sharedMesh, path + ".asset");
            AssetDatabase.SaveAssets();

            var newGO = Instantiate(meshFilter.gameObject, transform.parent, true);
            newGO.name = _mesh.name;

            // PrefabUtility.SaveAsPrefabAsset(newGO, path + ".prefab");

            // DestroyImmediate(newGO);
        }

        private void OnDrawGizmos()
        {
            foreach (var vg in vertexGroups)
            {
                var points = vg.Points;
                if (points.Length <= 2 || !_gizmos) return;

                for (var i = 0; i < points.Length - 1; i++)
                {
                    Gizmos.DrawLine(points[i].position, points[i + 1].position);
                    DrawString($"{i}", points[i].position, Color.cyan);
                }

                if (noFaces) continue;
                Gizmos.DrawLine(points[0].position, points[points.Length - 1].position);
                DrawString($"{points.Length - 1}", points[points.Length - 1].position, Color.cyan);
            }
        }

        private bool _gizmos = true;

        [ContextMenu(nameof(ToggleGizmos))]
        private void ToggleGizmos()
        {
            _gizmos = !_gizmos;
        }

        private static void DrawString(string text, Vector3 worldPos, Color? colour = null)
        {
            UnityEditor.Handles.BeginGUI();
            if (colour.HasValue) GUI.color = colour.Value;
            var view = UnityEditor.SceneView.currentDrawingSceneView;
            Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);
            Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));
            GUI.Label(new Rect(screenPos.x - (size.x / 2), -screenPos.y + view.position.height - 45, size.x, size.y),
                text);
            UnityEditor.Handles.EndGUI();
        }
#endif
    }
}