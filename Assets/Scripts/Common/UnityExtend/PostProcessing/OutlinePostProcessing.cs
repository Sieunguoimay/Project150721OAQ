using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Common.UnityExtend.PostProcessing
{
    public class OutlinePostProcessing : MonoBehaviour
    {
        [SerializeField] private Shader drawAsSolidColor;
        [SerializeField] private Shader outline;

        private Material _outlineMaterial;
        private Camera _outlineCamera;
        private int _mask;
        private static readonly int SceneTex = Shader.PropertyToID("_SceneTex");

        private void Start()
        {
            _outlineMaterial = new Material(outline);
            _outlineCamera = new GameObject("OutlineCamera").AddComponent<Camera>();
            _mask = 1 << LayerMask.NameToLayer($"Outline");
        }

        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            _outlineCamera.CopyFrom(Camera.current);
            _outlineCamera.backgroundColor = Color.black;
            _outlineCamera.clearFlags = CameraClearFlags.Color;
            _outlineCamera.cullingMask = _mask;
            var rt = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.R8);
            _outlineCamera.targetTexture = rt;
            _outlineCamera.RenderWithShader(drawAsSolidColor, "");
            _outlineMaterial.SetTexture(SceneTex, src);
            Graphics.Blit(rt, dest, _outlineMaterial);
            RenderTexture.ReleaseTemporary(rt);
        }
    }
}