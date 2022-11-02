using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Common.Noise
{
    public class NoiseTextureCreator : EditorWindow
    {
        [MenuItem("Tools/Noise Texture Creator")]
        public static void OpenWindow()
        {
            // Get existing open window or if none, make a new one:
            var window = (NoiseTextureCreator) GetWindow(typeof(NoiseTextureCreator));
            window.Show();
        }

        [Range(2, 512)] public int resolution = 256;
        public float frequency = 10f;
        [Range(1, 8)] public int octaves = 1;
        [Range(1, 3)] public int dimensions = 3;
        public NoiseMethodType type = NoiseMethodType.Perlin;
        public Gradient coloring = new();
        public string savePath;

        private Texture2D _texture;

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            resolution = EditorGUILayout.IntField(nameof(resolution), resolution);
            frequency = EditorGUILayout.FloatField(nameof(frequency), frequency);
            octaves = EditorGUILayout.IntField(nameof(octaves), octaves);
            dimensions = EditorGUILayout.IntField(nameof(dimensions), dimensions);
            type = (NoiseMethodType) EditorGUILayout.EnumPopup(nameof(type), type);
            coloring = EditorGUILayout.GradientField(nameof(coloring), coloring);
            savePath = EditorGUILayout.TextField(nameof(savePath) + " (Assets/)", savePath);

            if (GUILayout.Button(nameof(GenerateTexture)))
            {
                GenerateTexture();
            }

            if (GUILayout.Button(nameof(SaveToAsset)))
            {
                SaveToAsset();
            }

            EditorGUILayout.EndVertical();
            var rect = GUILayoutUtility.GetLastRect();
            var remainingRect = new Rect(0, rect.height, position.width, position.height - rect.height);
            if (_texture)
            {
                var size = Mathf.Min(Mathf.Max(_texture.height, _texture.width), Mathf.Min(remainingRect.width, remainingRect.height));
                EditorGUI.DrawPreviewTexture(new Rect(remainingRect.width / 2 - size / 2, remainingRect.y + remainingRect.height / 2 - size / 2, size, size), _texture);
            }
        }

        private void GenerateTexture()
        {
            if (_texture == null)
            {
                _texture = new Texture2D(resolution, resolution, TextureFormat.RGB24, true) {name = "Procedural Texture", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Trilinear, anisoLevel = 9};
                // GetComponent<MeshRenderer>().sharedMaterial.mainTexture = _texture;
            }

            FillTexture();
        }

        private void SaveToAsset()
        {
            var path = Path.Combine(Application.dataPath, $"{savePath}.png");
            File.WriteAllBytes(path, _texture.EncodeToPNG());
            AssetDatabase.Refresh();
        }

        public void FillTexture()
        {
            if (_texture.width != resolution)
            {
                _texture.Reinitialize(resolution, resolution);
            }

            var point00 = new Vector3(-0.5f, -0.5f);
            var point10 = new Vector3(0.5f, -0.5f);
            var point01 = new Vector3(-0.5f, 0.5f);
            var point11 = new Vector3(0.5f, 0.5f);

            var method = Noise.Methods[(int) type][dimensions - 1];
            var stepSize = 1f / resolution;
            for (var y = 0; y < resolution; y++)
            {
                var point0 = Vector3.Lerp(point00, point01, (y + 0.5f) * stepSize);
                var point1 = Vector3.Lerp(point10, point11, (y + 0.5f) * stepSize);
                for (var x = 0; x < resolution; x++)
                {
                    var point = Vector3.Lerp(point0, point1, (x + 0.5f) * stepSize);
                    var sample = Noise.Sum(method, point, frequency, octaves, 2f, .5f);
                    if (type != NoiseMethodType.Value)
                    {
                        sample = sample * 0.5f + 0.5f;
                    }

                    _texture.SetPixel(x, y, coloring.Evaluate(sample));
                }
            }

            _texture.Apply();
        }
    }
}