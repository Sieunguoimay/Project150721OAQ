using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;

namespace Common.Animation
{
    public static class AnimatorControllerHashesCreator
    {
        [MenuItem("Assets/Create/CreateHashes")]
        public static void CreateHashes()
        {
            var animatorController = (AnimatorController) Selection.activeObject;
            if (animatorController == null) return;
            
            var className = $"{CleanInput(animatorController.name)}Hashes";
            var fileContent = $"using UnityEngine;\npublic static class {className}\n{{\n";
            foreach (var p in animatorController.parameters)
            {
                fileContent += GetDeclaration(p.name);
            }

            foreach (var l in animatorController.layers)
            {
                foreach (var s in l.stateMachine.states)
                {
                    fileContent += GetDeclaration(s.state.name);
                }
            }

            fileContent += "\n}";

            var file = File.Create(Path.GetFullPath(
                Path.GetDirectoryName(AssetDatabase.GetAssetPath(animatorController)) +
                $"/{className}.cs"));
            file.Write(Encoding.ASCII.GetBytes(fileContent), 0, fileContent.Length);
            file.Close();
            AssetDatabase.Refresh();
        }

        private static string GetDeclaration(string name)
        {
            return $"\tpublic static readonly int {CleanInput(name)} = Animator.StringToHash(\"{name}\");\n";
        }

        private static string CleanInput(string strIn)
        {
            // Replace invalid characters with empty strings.
            try
            {
                return Regex.Replace(strIn, @"^[^A-Za-z_]+|\W+", "_");
            }
            // If we timeout when replacing invalid characters,
            // we should return Empty.
            catch (RegexMatchTimeoutException)
            {
                return String.Empty;
            }
        }
    }
}