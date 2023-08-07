using Common.UnityExtend.Attribute;
using UnityEditor;
using UnityEngine;

namespace Common.Tools.MeshSaver
{
    public interface IMeshProvider
    {
        UnityEngine.Mesh GetMesh();
    }

    public class SaveMeshFromRendererToAsset : MonoBehaviour, IMeshProvider
    {
        [SerializeField, TypeConstraint(typeof(IMeshProvider))]
        private Object meshProvider;

        [SerializeField] private string savePath = "Assets/GeneratedMesh/";
        [SerializeField] private string meshName = "Untitled";
        private IMeshProvider MeshProvider => meshProvider ? (IMeshProvider) meshProvider : this;

#if UNITY_EDITOR
        [ContextMenu("Save")]
        public void Save()
        {
            var mesh = MeshProvider.GetMesh();
            if (mesh == null)
            {
                Debug.LogError("Mesh is null!");
                return;
            }

            var path = $"{savePath}/{meshName}";

            AssetDatabase.CreateAsset(mesh, path + ".asset");
            AssetDatabase.SaveAssets();
            Debug.Log($"Saved mesh to {path}", AssetDatabase.LoadAssetAtPath<Object>(path));
        }
#endif
        public UnityEngine.Mesh GetMesh()
        {
            var m = GetMeshFromRenderer(GetComponent<Renderer>());
            return new Mesh
            {
                vertices =  m.vertices,
                triangles =  m.triangles,
                normals = m.normals,
                tangents = m.tangents,
                uv = m.uv,
                bounds = m.bounds,
                colors = m.colors
            };
        }

        public static Mesh GetMeshFromRenderer(Renderer renderer)
        {
            if (renderer is MeshRenderer mr)
            {
                return mr.GetComponent<MeshFilter>().sharedMesh;
            }

            if (renderer is SkinnedMeshRenderer smr)
            {
                var mesh = new Mesh();
                smr.BakeMesh(mesh);
                return mesh;
            }

            Debug.LogError($"This type of renderer is Not supported yet. Provide it here. {renderer.gameObject.name}");
            
            return null;
        }
    }
}