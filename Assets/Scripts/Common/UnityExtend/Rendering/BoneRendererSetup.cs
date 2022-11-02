using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Common.UnityExtend.Rendering
{
    public class BoneRendererSetup : MonoBehaviour
    {
        #if UNITY_EDITOR
        [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;

        [ContextMenu("Setup")]
        public void Setup()
        {
            var boneRenderer = gameObject.AddComponent<BoneRenderer>();
            var bones = skinnedMeshRenderer.bones;
            boneRenderer.transforms = new Transform[bones.Length];
            for (var i = 0; i < bones.Length; i++)
            {
                boneRenderer.transforms[i] = bones[i];
            }
            DestroyImmediate(this);
        }
        #endif
    }
}