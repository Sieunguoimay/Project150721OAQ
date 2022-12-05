using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Common.UnityExtend.Rendering.Water
{
    /// <summary>
    /// 水面
    /// </summary>
    [AddComponentMenu("GameCore/Effect/Water/Water (Base)")]
    // [ExecuteInEditMode]
    public class WaterSurface : MonoBehaviour
    {
        public enum FlageWaterRefType
        {
            Both = 0,
            Reflection = 1,
            Refraction = 2
        }

        public bool disablePixelLights = false;
        public LayerMask layers = -1;
        public int texSize = 512;
        public FlageWaterRefType refType = FlageWaterRefType.Both;
        public float reflectClipPlaneOffset = 0;
        public float refractionAngle = 0;

        private static Camera _reflectionCamera;
        private static Camera _refractionCamera;

        private int _oldTexSize = 0;
        private RenderTexture _reflectionRenderTex;
        private RenderTexture _refractionRenderTex;

        private bool _insideRendering = false;
        private float _refType = (float) FlageWaterRefType.Both;


        private static readonly int ReflectionTex = Shader.PropertyToID("_ReflectionTex");
        private static readonly int RefractionTex = Shader.PropertyToID("_RefractionTex");
        private static readonly int ProjMatrix = Shader.PropertyToID("_ProjMatrix");
        private static readonly int RefType = Shader.PropertyToID("_RefType");

        private Renderer _renderer;
        private static readonly int MainLightDirection = Shader.PropertyToID("_MainLightDirection");
        private Renderer Renderer => _renderer ??= GetComponent<Renderer>();

        private void OnWillRenderObject()
        {
            if (!enabled || !Renderer || !Renderer.sharedMaterial || !Renderer.enabled)
                return;
            var cam = Camera.current;
            if (!cam)
                return;

            if (_insideRendering)
                return;

            _insideRendering = true;

            var oldPixelLightCount = QualitySettings.pixelLightCount;

            if (disablePixelLights)
                QualitySettings.pixelLightCount = 0;

            var materials = Renderer.sharedMaterials;

            if (refType is FlageWaterRefType.Both or FlageWaterRefType.Reflection)
            {
                DrawReflectionRenderTexture(cam);
                foreach (var mat in materials)
                {
                    if (mat.HasProperty(ReflectionTex))
                        mat.SetTexture(ReflectionTex, _reflectionRenderTex);
                }
            }

            if (refType is FlageWaterRefType.Both or FlageWaterRefType.Refraction)
            {
                gameObject.layer = 4;
                DrawRefractionRenderTexture(cam);
                foreach (var mat in materials)
                {
                    if (mat.HasProperty(RefractionTex))
                        mat.SetTexture(RefractionTex, _refractionRenderTex);
                }
            }

            _refType = (float) refType;
            var projmtx = CoreTool.UV_Tex2DProj2Tex2D(transform, cam);
            var sunMatrix = RenderSettings.sun.transform.localToWorldMatrix;
            foreach (var mat in materials)
            {
                mat.SetMatrix(ProjMatrix, projmtx);
                mat.SetFloat(RefType, _refType);
                mat.SetMatrix(MainLightDirection, sunMatrix);
            }

            if (disablePixelLights)
                QualitySettings.pixelLightCount = oldPixelLightCount;

            _insideRendering = false;
        }

        private void DrawReflectionRenderTexture(Camera cam)
        {
            CreateRenderTexture(ref _reflectionRenderTex);
            CreateObjects(cam, ref _reflectionCamera);
            CoreTool.CloneCameraModes(cam, _reflectionCamera);

            var thisTransform = transform;
            var pos = thisTransform.position;
            var normal = thisTransform.up;

            var d = -Vector3.Dot(normal, pos) - reflectClipPlaneOffset;
            var reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);
            var reflection = CoreTool.CalculateReflectionMatrix(Matrix4x4.zero, reflectionPlane);

            var oldPos = cam.transform.position;
            _reflectionCamera.worldToCameraMatrix = cam.worldToCameraMatrix * reflection;

            // Setup oblique projection matrix so that near plane is our reflection
            // plane. This way we clip everything below/above it for free.
            var clipPlane = CoreTool.CameraSpacePlane(_reflectionCamera, pos, normal, 1.0f, reflectClipPlaneOffset);

            var projection = cam.projectionMatrix;

            projection = CoreTool.CalculateObliqueMatrix(projection, clipPlane, -1);

            _reflectionCamera.projectionMatrix = projection;
            _reflectionCamera.cullingMask = ~(1 << 4) & layers.value; // never render water layer
            _reflectionCamera.SetTargetBuffers(_reflectionRenderTex.colorBuffer, _reflectionRenderTex.depthBuffer);
            // _reflectionCamera.targetTexture = _reflectionRenderTex;

            var reflectionCameraTransform = _reflectionCamera.transform;
            var euler = cam.transform.eulerAngles;

            reflectionCameraTransform.position = reflection.MultiplyPoint(oldPos);
            reflectionCameraTransform.eulerAngles = new Vector3(0, euler.y, euler.z);

            GL.invertCulling = true;
            _reflectionCamera.Render();
            GL.invertCulling = false;

            _reflectionCamera.transform.position = oldPos;
        }

        private void DrawRefractionRenderTexture(Camera cam)
        {
            CreateRenderTexture(ref _refractionRenderTex);
            CreateObjects(cam, ref _refractionCamera);
            CoreTool.CloneCameraModes(cam, _refractionCamera);

            var thisTransform = transform;
            var pos = thisTransform.position;
            var normal = thisTransform.up;

            var projection = cam.worldToCameraMatrix;
            projection *= Matrix4x4.Scale(new Vector3(1, Mathf.Clamp(1 - refractionAngle, 0.001f, 1), 1));
            _refractionCamera.worldToCameraMatrix = projection;

            var clipPlane = CoreTool.CameraSpacePlane(_refractionCamera, pos, -normal, 1.0f, 0);

            // projection[2] = clipPlane.x + projection[3]; //x
            // projection[6] = clipPlane.y + projection[7]; //y
            // projection[10] = clipPlane.z + projection[11]; //z
            // projection[14] = clipPlane.w + projection[15]; //w

            // projection = CoreTool.CalculateObliqueMatrix(projection, clipPlane, -1);

            _refractionCamera.projectionMatrix = _refractionCamera.CalculateObliqueMatrix(clipPlane);
            _refractionCamera.cullingMask = ~(1 << 4) & layers.value; // never render water layer
            _refractionCamera.SetTargetBuffers(_refractionRenderTex.colorBuffer, _refractionRenderTex.depthBuffer);
            // _refractionCamera.targetTexture = _refractionRenderTex;

            var mainCamTransform = cam.transform;
            var refractionCameraTransform = _refractionCamera.transform;
            refractionCameraTransform.position = mainCamTransform.position;
            refractionCameraTransform.eulerAngles = mainCamTransform.eulerAngles;

            _refractionCamera.Render();
        }

        private void OnDisable()
        {
            if (_reflectionRenderTex)
            {
                DestroyImmediate(_reflectionRenderTex);
                _reflectionRenderTex = null;
            }

            if (_reflectionCamera)
            {
                DestroyImmediate(_reflectionCamera.gameObject);
                _reflectionCamera = null;
            }

            if (_refractionRenderTex)
            {
                DestroyImmediate(_refractionRenderTex);
                _refractionRenderTex = null;
            }

            if (_refractionCamera)
            {
                DestroyImmediate(_refractionCamera.gameObject);
                _refractionCamera = null;
            }
        }

        private void CreateRenderTexture(ref RenderTexture renderTex)
        {
            // Reflection render texture
            if (!renderTex || _oldTexSize != texSize)
            {
                if (renderTex)
                    DestroyImmediate(renderTex);
                renderTex = new RenderTexture(texSize, texSize, 24);
                renderTex.name = "__RefRenderTexture" + renderTex.GetInstanceID();
                renderTex.isPowerOfTwo = true;
                renderTex.hideFlags = HideFlags.DontSave;
                renderTex.antiAliasing = 4;
                renderTex.anisoLevel = 0;
                _oldTexSize = texSize;
            }
        }

        private void CreateObjects(Camera srcCam, ref Camera destCam)
        {
            if (!destCam) // catch both not-in-dictionary and in-dictionary-but-deleted-GO
            {
                var go = new GameObject("__RefCamera for " + srcCam.GetInstanceID(), typeof(Camera), typeof(Skybox));
                destCam = go.GetComponent<Camera>();
                destCam.enabled = false;
                destCam.transform.position = transform.position;
                destCam.transform.rotation = transform.rotation;
                destCam.gameObject.AddComponent<FlareLayer>();
                go.hideFlags = HideFlags.HideAndDontSave;
            }
        }
    }
}