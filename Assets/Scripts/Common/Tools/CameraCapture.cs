#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Common.Tools
{
    public static class CameraCapture
    {
        [MenuItem("Tools/CaptureCamera")]
        public static void Capture()
        {
            // var currentDateTime =
            //     DateTime.Now.ToString("yyyyMMdd_Hmmss");

            //You may want to use Application.persistentDataPath
            // var directory = new DirectoryInfo(Application.dataPath + "\\Assets");
            //
            // var path = Path.Combine(directory.Parent.FullName, $"Screenshot_{currentDateTime}.png");
            //
            // Debug.Log(path);

            ScreenCapture.CaptureScreenshot(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                $"Screenshot_{DateTime.Now:yyyyMMdd_Hmmss}.png"));
        }
    }
}
#endif