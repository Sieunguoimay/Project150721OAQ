using System.Collections.Generic;
using Common.UnityExtend.Attribute;
using Common.UnityExtend.Rendering;
using UnityEngine;
using UnityEngine.Rendering;


namespace Contents.Shaders.SupportScripts
{
    [RequireComponent(typeof(MeshRenderer))]
    public class ShaderPropertySetter : MonoBehaviour
    {
#if UNITY_EDITOR
        [StringSelector(nameof(PropertyNames))]
#endif
        [SerializeField]
        private string propertyName;

        private MeshRenderer _meshRenderer;
        private int _propertyID;

#if UNITY_EDITOR
        private List<string> PropertyNames
        {
            get
            {
                if (_meshRenderer == null)
                {
                    _meshRenderer = GetComponent<MeshRenderer>();
                }

                return ShaderUtility.GetShaderPropertyNamesByType(_meshRenderer.sharedMaterial.shader, ShaderPropertyType.Range, ShaderPropertyType.Float);
            }
        }

#endif
        public void SetProperty(float value)
        {
            if (_meshRenderer == null)
            {
                _meshRenderer = GetComponent<MeshRenderer>();
                _propertyID = Shader.PropertyToID(propertyName);
                if (!_meshRenderer.sharedMaterial.HasFloat(_propertyID))
                {
                    Debug.Log($"Err. Property {propertyName} not found in this Shader {_meshRenderer.sharedMaterial.shader.name}");
                }
            }

            _meshRenderer.material.SetFloat(_propertyID, value);
        }
    }
}