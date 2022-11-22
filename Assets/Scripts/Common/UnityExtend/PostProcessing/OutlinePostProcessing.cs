using System;
using System.Collections.Generic;
using System.Linq;
using Common.UnityExtend.Attribute;
using UnityEditor;
using UnityEngine;

namespace Common.UnityExtend.PostProcessing
{
    public class LayerSelectorAttribute : StringSelectorAttribute
    {
        public LayerSelectorAttribute() : base("")
        {
        }
    }
#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(LayerSelectorAttribute))]
    public class LayerSelectorDrawer : StringSelectorDrawer
    {
        protected override IEnumerable<string> GetIds(SerializedProperty property,
            StringSelectorAttribute objectSelector)
        {
            return Enumerable.Range(0, 31).Select(LayerMask.LayerToName).Where(l => !string.IsNullOrEmpty(l));
        }
    }
#endif
    public class OutlinePostProcessing : MonoBehaviour
    {
        [SerializeField] private Shader outline;

        [SerializeField, LayerSelector] private string layerMask = "Outline";

        [SerializeField, StringSelector(nameof(Keywords))]
        private string outlineSizeKeyword;

        public string[] Keywords => new[] {"OUTLINE_12", "OUTLINE_9", "OUTLINE_5"};

        private Material _outlineMaterial;
        private Camera _outlineCamera;
        private int _mask;
        private static readonly int SceneTex = Shader.PropertyToID("_SceneTex");


        private void Start()
        {
            _outlineMaterial = new Material(outline);
            _outlineMaterial.EnableKeyword(outlineSizeKeyword);

            _outlineCamera = new GameObject("OutlineCamera").AddComponent<Camera>();
            // _outlineCamera.gameObject.hideFlags = HideFlags.HideAndDontSave;
            _mask = 1 << LayerMask.NameToLayer(layerMask);
        }

        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            _outlineCamera.CopyFrom(Camera.current);
            _outlineCamera.backgroundColor = Color.black;
            _outlineCamera.clearFlags = CameraClearFlags.Color;
            _outlineCamera.cullingMask = _mask;
            var rt = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.R8);
            _outlineCamera.targetTexture = rt;
            _outlineCamera.Render();

            _outlineMaterial.SetTexture(SceneTex, src);
            Graphics.Blit(rt, dest, _outlineMaterial);
            RenderTexture.ReleaseTemporary(rt);
        }
    }
}