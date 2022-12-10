using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Text3D.Scripts
{
    [Serializable]
    public enum VerticalAlignment
    {
        Top,
        Mid,
        Bottom
    }

    [Serializable]
    public enum HorizontalAlignment
    {
        Left,
        Center,
        Right
    }

    [ExecuteInEditMode]
    public class Text3D : MonoBehaviour
    {
        [SerializeField] private Text3DFont sourceFont = null;
        [SerializeField] [TextArea(3, 5)] private string inputText = "";
        [SerializeField] private int fontSize = 1;
        [SerializeField] private float characterSpace;
        [SerializeField] private float wordSpace;
        [SerializeField] private float lineSpace;
        [SerializeField] private VerticalAlignment verticalAlignment;
        [SerializeField] private HorizontalAlignment horizontalAlignment;
        [SerializeField] private Material material;
        [SerializeField] private bool receiveShadows = true;
        [SerializeField] private ShadowCastingMode castShadows = ShadowCastingMode.On;
#if UNITY_EDITOR
        public Text3DFont SourceFont => sourceFont;
#endif

        private List<Character3DRenderer> _characterRenderers = new();

        private void OnValidate()
        {
            if (_characterRenderers.Count == 0 && inputText.Length > 0)
            {
                _characterRenderers = GetComponentsInChildren<Character3DRenderer>(true).ToList();
                GenerateText(true);
            }
        }

        public void GenerateText(bool clear)
        {
            if (clear)
            {
                foreach (var cr in _characterRenderers)
                {
                    cr.gameObject.SetActive(false);
                }
            }

            if (null == sourceFont || string.IsNullOrEmpty(inputText))
                return;

            var it = 0;
            var count = 0;
            var fSize = fontSize / (float) sourceFont.UnitsPerEm;
            var wSpace = sourceFont.WordSpace.x * fSize + wordSpace;
            var lSpace = sourceFont.LineSpace.x * fSize + lineSpace;
            var pos = new Vector3(0, -lSpace);
            var scale = new Vector3(fSize, fSize, fSize);

            var maxWidth = float.MinValue;
            while (it < inputText.Length)
            {
                pos.x = 0.0f;

                var inLineStartIndex = count;
                while (it < inputText.Length && '\n' != inputText[it])
                {
                    var curr = inputText[it++];
                    var next = inputText.Length != it ? inputText[it] : '\0';

                    if (' ' == curr)
                    {
                        pos.x += wSpace;
                        continue;
                    }

                    var glyph = sourceFont.GetGlyph(curr);

                    if (null == glyph)
                        continue;

                    GameObject go;
                    if (clear)
                    {
                        if (count >= _characterRenderers.Count)
                        {
                            _characterRenderers.Add(Character3DRenderer.CreateNew(transform));
                        }

                        var cr = _characterRenderers[count++];
                        cr.Setup(glyph, curr);
                        (go = cr.gameObject).SetActive(true);
                        cr.MeshRenderer.sharedMaterial = material;
                        cr.MeshRenderer.shadowCastingMode = castShadows;
                        cr.MeshRenderer.receiveShadows = receiveShadows;
                    }
                    else
                    {
                        go = _characterRenderers[count++].gameObject;
                    }

                    go.layer = gameObject.layer;
                    go.isStatic = gameObject.isStatic;
                    go.transform.localRotation = Quaternion.identity;
                    go.transform.localPosition = pos;
                    go.transform.localScale = scale;

                    var cSpace = characterSpace + (glyph.advance.x + sourceFont.GetKerning(curr, next, false)) * fSize;
                    pos.x += cSpace;
                }

                if (it < inputText.Length)
                    ++it;

                if (maxWidth < pos.x)
                {
                    maxWidth = pos.x;
                }

                for (var i = inLineStartIndex; i < count; i++)
                {
                    AlignHorizontal(_characterRenderers[i].transform, pos.x);
                }

                pos.y -= lSpace;
            }

            pos.y += lSpace;

            for (var i = 0; i < count; i++)
            {
                AlignVertical(_characterRenderers[i].transform, pos.y);
            }
        }

        private void AlignVertical(Transform t, float height)
        {
            if (verticalAlignment == VerticalAlignment.Top) return;

            var lp = t.localPosition;

            switch (verticalAlignment)
            {
                case VerticalAlignment.Mid:
                    lp.y -= height / 2;
                    break;
                case VerticalAlignment.Bottom:
                    lp.y -= height;
                    break;
                case VerticalAlignment.Top:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            t.localPosition = lp;
        }

        private void AlignHorizontal(Transform t, float width)
        {
            if (horizontalAlignment == HorizontalAlignment.Left) return;

            var p = t.localPosition;
            switch (horizontalAlignment)
            {
                case HorizontalAlignment.Center:
                    p.x -= width / 2;
                    break;
                case HorizontalAlignment.Right:
                    p.x -= width;
                    break;
                case HorizontalAlignment.Left:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            t.localPosition = p;
        }

        public void SetText(string text, bool update)
        {
            inputText = text;

            if (update)
                GenerateText(true);
        }

#if UNITY_EDITOR
        public void SetFont(Text3DFont font, bool update)
        {
            sourceFont = font;

            if (update)
                GenerateText(true);
        }
#endif
    }
}