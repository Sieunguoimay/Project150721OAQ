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
            string currentDateTime =
                DateTime.Now.ToString("yyyyMMdd_Hmmss");

            //You may want to use Application.persistentDataPath
            var directory = new DirectoryInfo(Application.dataPath + "\\Assets\\Screenshots");

            var path = Path.Combine(directory.Parent.FullName, $"Screenshot_{currentDateTime}.png");

            Debug.Log(path);

            ScreenCapture.CaptureScreenshot(path);
        }
    }
}