using System;
using UnityEngine;

namespace Common.UnityExtend.Rendering
{
    public class InstancedRendering : MonoBehaviour
    {
        [SerializeField] private Mesh mesh;
        [SerializeField] private Material material;
        [SerializeField] private Matrix4x4[] instances;
        [SerializeField] private Bounds bounds;

        private ComputeBuffer _argsBuffer;
        private ComputeBuffer _matricesBuffer;

        private void Start()
        {
            _argsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
            _argsBuffer.SetData(new uint[]
            {
                (uint) mesh.GetIndexCount(0), // triangle indices count per instance
                (uint) instances.Length, // instance count
                (uint) mesh.GetIndexStart(0), // start index location
                (uint) mesh.GetBaseVertex(0), // base vertex location
                0, // start instance location
            });

            _matricesBuffer = new ComputeBuffer(instances.Length, InstanceData.GetSize());
            _matricesBuffer.SetData(instances);
            material.SetBuffer("matricesBuffer", _matricesBuffer);
        }

        private void OnDestroy()
        {
            _matricesBuffer.Release();
            _matricesBuffer = null;

            _argsBuffer.Release();
            _argsBuffer = null;
        }

        private void Update()
        {
            Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, _argsBuffer);
        }

        [ContextMenu("To GameObject")]
        private void ToGameObject()
        {
            ClearChildrenGameObjects();
            var thisTransform = transform;

            for (var i = 0; i < instances.Length; i++)
            {
                var m = instances[i];
                var t = new GameObject("").transform;
                t.SetParent(transform);

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

            var minMaxX = new Vector2(float.MaxValue, float.MinValue);
            var minMaxY = new Vector2(float.MaxValue, float.MinValue);
            var minMaxZ = new Vector2(float.MaxValue, float.MinValue);
            instances = new Matrix4x4[transforms.Length - 1];
            var index = 0;
            for (var i = 0; i < transforms.Length; i++)
            {
                var t = transforms[i];
                if (t != thisTransform)
                {
                    instances[index++] = t.localToWorldMatrix;
                    minMaxX = new Vector2(Mathf.Min(t.position.x, minMaxX.x), Mathf.Max(t.position.x, minMaxX.y));
                    minMaxY = new Vector2(Mathf.Min(t.position.y, minMaxX.x), Mathf.Max(t.position.y, minMaxX.y));
                    minMaxZ = new Vector2(Mathf.Min(t.position.z, minMaxX.x), Mathf.Max(t.position.z, minMaxX.y));
                }
            }

            bounds = new Bounds(
                new Vector3((minMaxX.x + minMaxX.y) / 2f, (minMaxY.x + minMaxY.y) / 2f, (minMaxZ.x + minMaxZ.y) / 2f),
                new Vector3(Mathf.Max(1f, (minMaxX.y - minMaxX.x) / 2f),
                    Mathf.Max(1f, (minMaxY.y - minMaxY.x) / 2f),
                    Mathf.Max(1f, (minMaxZ.y - minMaxZ.x) / 2f)));
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