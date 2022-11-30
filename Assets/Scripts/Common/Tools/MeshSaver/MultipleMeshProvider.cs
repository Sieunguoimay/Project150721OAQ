using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Common.Tools.MeshSaver
{
    public class MultipleMeshProvider : MonoBehaviour, IMeshProvider
    {
        [SerializeField] private Renderer[] renderers;

        public Renderer[] Renderers => renderers;

        public Mesh GetMesh()
        {
            var mesh = new Mesh();
            mesh.CombineMeshes(renderers.Select(r => new CombineInstance
            {
                mesh = SaveMeshFromRendererToAsset.GetMeshFromRenderer(r),
                transform = transform.worldToLocalMatrix * r.transform.localToWorldMatrix
            }).ToArray());
            return mesh;
        }

        [ContextMenu(nameof(GetFromChildren))]
        private void GetFromChildren()
        {
            renderers = GetComponentsInChildren<Renderer>();
        }
    }
}