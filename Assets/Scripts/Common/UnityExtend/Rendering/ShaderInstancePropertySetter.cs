using System;
using UnityEngine;

namespace Common.UnityExtend.Rendering
{
    public class ShaderInstancePropertySetter : MonoBehaviour
    {
        [SerializeField] private MeshRenderer target;
        [SerializeField] private string propName;
        [SerializeField] private float value;
        private MaterialPropertyBlock _block;

        private string[]
        
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
            _block.SetFloat(propName, value);
            target.SetPropertyBlock(_block);
        }
    }
}