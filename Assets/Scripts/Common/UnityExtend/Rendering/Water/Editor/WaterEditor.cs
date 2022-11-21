using UnityEditor;
using UnityEngine;

// [CustomEditor(typeof(Water))]
// public class WaterEditor : UnityEditor.Editor
// {
//     private readonly GUIContent[] _renderTextureOptions = new GUIContent[8]
//     {
//         new GUIContent("16"), new GUIContent("32"), new GUIContent("64"), new GUIContent("128"),
//         new GUIContent("256"), new GUIContent("512"), new GUIContent("1024"), new GUIContent("2048")
//     };
//
//     private readonly int[] _renderTextureSize = new int[8] {16, 32, 64, 128, 256, 512, 1024, 2048};
//
//     public override void OnInspectorGUI()
//     {
//         // var water = target as Water;
//         // EditorGUILayout.PropertyField(this.serializedObject.FindProperty("RefType"), new GUIContent("RefType"));
//         // EditorGUILayout.PropertyField(this.serializedObject.FindProperty("DisablePixelLights"),
//         //     new GUIContent("DisablePixelLights"));
//         // EditorGUILayout.PropertyField(this.serializedObject.FindProperty("Layers"), new GUIContent("Layers"));
//         // EditorGUILayout.IntPopup(this.serializedObject.FindProperty("TexSize"), _renderTextureOptions,
//         //     _renderTextureSize, new GUIContent("TexSize"));
//         //
//         //
//         // EditorGUILayout.LabelField("Reflect Settings");
//         // EditorGUILayout.Slider(this.serializedObject.FindProperty("ReflectClipPlaneOffset"), 0, 0.1f,
//         //     new GUIContent("ClipPlane Offset"));
//         //
//         // EditorGUILayout.LabelField("Refraction Settings");
//         // EditorGUILayout.Slider(this.serializedObject.FindProperty("RefractionAngle"), 0, 1,
//         //     new GUIContent("Refraction Angle"));
//         //
//         // this.serializedObject.ApplyModifiedProperties();
//     }
// }