using UnityEngine;

namespace Text3D.Scripts
{
    public class Character3DRenderer : MonoBehaviour
    {
        [field: SerializeField, HideInInspector]
        public MeshFilter MeshFilter { get; private set; }

        [field: SerializeField, HideInInspector]
        public MeshRenderer MeshRenderer { get; private set; }

        [field: System.NonSerialized] public Text3DGlyph Glyph { get; private set; }

        public void Setup(Text3DGlyph glyph, char c)
        {
            Glyph = glyph;
            MeshFilter.sharedMesh = glyph.mesh;
            gameObject.name = $"character_{c}";
        }

        public static Character3DRenderer CreateNew(Transform parent)
        {
            var go = new GameObject("character");
            go.transform.parent = parent;

            go.SetActive(false);
            var cr = go.AddComponent<Character3DRenderer>();
            cr.MeshFilter = go.AddComponent<MeshFilter>();
            cr.MeshRenderer = go.AddComponent<MeshRenderer>();
            go.SetActive(true);
            return cr;
        }
    }
}