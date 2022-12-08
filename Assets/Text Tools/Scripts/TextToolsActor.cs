// Copyright (C) 2019 Alexander Klochkov - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms


using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using System.Collections.Generic;


namespace texttools
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
    public class TextToolsActor : MonoBehaviour
    {
        [SerializeField] private TextToolsFont sourceFont = null;
        [SerializeField] [TextArea(3, 5)] private string inputText = "";
        [SerializeField] private int fontSize = 72;
        [SerializeField] private float characterSpace;
        [SerializeField] private float wordSpace;
        [SerializeField] private float lineSpace;
        [SerializeField] private VerticalAlignment verticalAlignment;
        [SerializeField] private HorizontalAlignment horizontalAlignment;
        [SerializeField] private Material material;
        [SerializeField] private bool receiveShadows = true;
        [SerializeField] private ShadowCastingMode castShadows = ShadowCastingMode.On;
#if UNITY_EDITOR

        public TextToolsFont SourceFont => sourceFont;

#endif

        public void GenerateText(bool clear)
        {
            GameObject go;
            MeshFilter mf;
            MeshRenderer mr;

            if (clear)
            {
                for (int i = transform.childCount - 1; i >= 0; --i)
                {
                    go = transform.GetChild(i).gameObject;
                    mf = go.GetComponent<MeshFilter>();
                    mr = go.GetComponent<MeshRenderer>();
                    var mc = go.GetComponent<MeshCollider>();
                    var bc = go.GetComponent<BoxCollider>();
                    var rb = go.GetComponent<Rigidbody>();

                    mf.sharedMesh = null;
                    mr.enabled = false;

                    if (Application.isPlaying)
                    {
                        if (null != bc)
                        {
                            bc.enabled = false;
                            Destroy(bc);
                        }

                        if (null != mc)
                        {
                            mc.enabled = false;
                            Destroy(mc);
                        }

                        if (null != rb)
                            Destroy(rb);

                        Destroy(mr);
                        Destroy(mf);
                        Destroy(go);
                    }
                    else
                    {
                        if (null != bc)
                        {
                            bc.enabled = false;
                            DestroyImmediate(bc);
                        }

                        if (null != mc)
                        {
                            mc.enabled = false;
                            DestroyImmediate(mc);
                        }

                        if (null != rb)
                            DestroyImmediate(rb);

                        DestroyImmediate(mr);
                        DestroyImmediate(mf);
                        DestroyImmediate(go);
                    }
                }

                Resources.UnloadUnusedAssets();
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

            var gos = new List<GameObject>();
            var maxWidth = float.MinValue;
            while (it < inputText.Length)
            {
                pos.x = 0.0f;
                var gosInALine = new List<GameObject>();

                while (it < inputText.Length && '\n' != inputText[it])
                {
                    var curr = inputText[it++];
                    var next = (inputText.Length != it) ? inputText[it] : '\0';

                    if (' ' == curr)
                    {
                        pos.x += wSpace;
                        continue;
                    }

                    var glyph = sourceFont.GetGlyph(curr);

                    if (null == glyph)
                        continue;

                    if (clear)
                    {
                        go = new GameObject("C_" + curr + "_" + (count++));
                        go.transform.parent = transform;

                        mf = go.AddComponent<MeshFilter>();
                        mr = go.AddComponent<MeshRenderer>();

                        mf.sharedMesh = glyph.mesh;
                        mr.sharedMaterial = material;
                        mr.shadowCastingMode = castShadows;
                        mr.receiveShadows = receiveShadows;
                    }
                    else
                    {
                        go = transform.GetChild(count++).gameObject;
                    }

                    go.layer = gameObject.layer;
                    go.isStatic = gameObject.isStatic;
                    go.transform.localRotation = Quaternion.identity;
                    go.transform.localPosition = pos;
                    go.transform.localScale = scale;
                    gos.Add(go);
                    gosInALine.Add(go);

                    var cSpace = characterSpace + (glyph.advance.x + sourceFont.GetKerning(curr, next, false)) * fSize;
                    pos.x += cSpace;
                }

                if (it < inputText.Length)
                    ++it;

                if (maxWidth < pos.x)
                {
                    maxWidth = pos.x;
                }

                AlignHorizontal(gosInALine, pos.x);

                pos.y -= lSpace;
            }

            pos.y += lSpace;

            AlignVertical(gos, pos.y);
        }

        private void AlignVertical(IEnumerable<GameObject> gos, float height)
        {
            if (verticalAlignment != VerticalAlignment.Top)
            {
                foreach (var o in gos)
                {
                    var lp = o.transform.localPosition;

                    if (verticalAlignment == VerticalAlignment.Mid)
                    {
                        lp.y -= height / 2;
                    }
                    else if (verticalAlignment == VerticalAlignment.Bottom)
                    {
                        lp.y -= height;
                    }

                    o.transform.localPosition = lp;
                }
            }
        }

        private void AlignHorizontal(IEnumerable<GameObject> gosInALine, float width)
        {
            if (horizontalAlignment != HorizontalAlignment.Left)
            {
                foreach (var g in gosInALine)
                {
                    var p = g.transform.localPosition;
                    if (horizontalAlignment == HorizontalAlignment.Center)
                    {
                        p.x -= width / 2;
                    }
                    else if (horizontalAlignment == HorizontalAlignment.Right)
                    {
                        p.x -= width;
                    }

                    g.transform.localPosition = p;
                }
            }
        }

        public void SetText(string text, bool update)
        {
            inputText = text;

            if (update)
                GenerateText(true);
        }

        public void SetFont(TextToolsFont font, bool update)
        {
            sourceFont = font;

            if (update)
                GenerateText(true);
        }
    }
}