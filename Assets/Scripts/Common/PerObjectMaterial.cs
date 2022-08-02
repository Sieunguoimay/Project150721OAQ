using UnityEngine;

namespace Common
{
    [RequireComponent(typeof(MeshRenderer))]
    public class PerObjectMaterial : MonoBehaviour
    {
        [SerializeField] private Color color;
        private static MaterialPropertyBlock _block;
        private static readonly int ColorId = Shader.PropertyToID("_BaseColor");

        private MeshRenderer _meshRenderer;
        private MeshRenderer MeshRenderer => _meshRenderer ? _meshRenderer : _meshRenderer = GetComponent<MeshRenderer>();

        public Color Color
        {
            get => color;
            set
            {
                color = value;
                OnValidate();
            }
        }

        private void Awake()
        {
            OnValidate();
        }

        private void OnValidate()
        {
            _block ??= new MaterialPropertyBlock();

            _block.SetColor(ColorId, color);
            MeshRenderer.SetPropertyBlock(_block);
        }
    }
}