using System;
using System.Collections.Generic;
using System.Linq;
using Common.UnityExtend.Attribute;
using UnityEngine;
using UnityEngine.Rendering;

namespace Common.UnityExtend.Rendering
{
    public static class ShaderUtility
    {
        public static string[] GetShaderPropertyNames(Shader shader)
        {
            var propCount = shader.GetPropertyCount();
            var propNames = new string[propCount];
            for (var i = 0; i < propCount; i++)
            {
                propNames[i] = shader.GetPropertyName(i);
            }

            return propNames;
        }

        public static List<string> GetShaderPropertyNamesByType(Shader shader, params ShaderPropertyType[] types)
        {
            var propCount = shader.GetPropertyCount();
            var propNames = new List<string>();
            for (var i = 0; i < propCount; i++)
            {
                if (types.Contains(shader.GetPropertyType(i)))
                {
                    propNames.Add(shader.GetPropertyName(i));
                }
            }

            return propNames;
        }
    }

    public class ShaderInstancePropertySetter : MonoBehaviour
    {
        [SerializeField] private MeshRenderer target;

        [SerializeField] private Item[] items;

        private MaterialPropertyBlock _block;

        private List<string> PropertyNames => target ? ShaderUtility.GetShaderPropertyNamesByType(target.sharedMaterial.shader, ShaderPropertyType.Range, ShaderPropertyType.Float) : null;

        private void Start()
        {
            Set();
        }

        [ContextMenu("Set")]
        public void Set()
        {
            if (_block == null)
            {
                _block = new MaterialPropertyBlock();
            }

            target.GetPropertyBlock(_block);
            foreach (var i in items)
            {
                _block.SetFloat(i.PropName, i.Value);
            }

            target.SetPropertyBlock(_block);
        }

        [Serializable]
        private class Item
        {
            [field: SerializeField, StringSelector(nameof(PropertyNames), true)]
            public string PropName { get; private set; }

            [field: SerializeField] public float Value { get; private set; }
        }
    }
}