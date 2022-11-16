using System;
using UnityEngine;

namespace Common.UnityExtend.Rendering
{
    public class InstancedRendering : MonoBehaviour
    {
        [SerializeField] private Mesh mesh;
        [SerializeField] private Material material;
        [SerializeField] private Matrix4x4[] instances;

        private void Update()
        {
            Graphics.DrawMeshInstanced(mesh, 0, material, instances);
        }

        [ContextMenu("To GameObject")]
        private void ToGameObject()
        {
            ClearChildrenGameObjects();
            var thisTransform = transform;

            for (var i = 0; i < instances.Length; i++)
            {
                var m = instances[i];
                var t = new GameObject($"(Instance) ({i})").transform;
                t.SetParent(transform);
                t.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
                t.gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;

                // Extract new local position
                t.localPosition = thisTransform.InverseTransformPoint(m.GetColumn(3));

                // Extract new local rotation
                t.localRotation = Quaternion.LookRotation(
                    thisTransform.InverseTransformDirection(m.GetColumn(2)),
                    thisTransform.InverseTransformDirection(m.GetColumn(1))
                );

                // Extract new local scale
                t.localScale = new Vector3(
                    m.GetColumn(0).magnitude,
                    m.GetColumn(1).magnitude,
                    m.GetColumn(2).magnitude
                );
            }
        }

        [ContextMenu("From GameObject")]
        private void FromGameObject()
        {
            var transforms = GetComponentsInChildren<Transform>();
            var thisTransform = transform;

            instances = new Matrix4x4[transforms.Length - 1];
            var index = 0;
            for (var i = 0; i < transforms.Length; i++)
            {
                var t = transforms[i];
                if (t != thisTransform)
                {
                    instances[index++] = t.localToWorldMatrix;
                }
            }
        }

        [ContextMenu("Clear Children GameObjects")]
        private void ClearChildrenGameObjects()
        {
            var transforms = GetComponentsInChildren<Transform>();
            var thisTransform = transform;
            for (var i = 0; i < transforms.Length; i++)
            {
                var t = transforms[i];
                if (t != thisTransform)
                {
                    DestroyImmediate(t.gameObject);
                }
            }
        }

        [Serializable]
        public class InstanceData
        {
            public InstanceData(Matrix4x4 m) => Matrix = m;
            [field: SerializeField] public Matrix4x4 Matrix { get; private set; }

            public static int GetSize()
            {
                return sizeof(float) * 4 * 4;
            }
        }
    }
}