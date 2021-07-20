using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class PerObjectMaterial : MonoBehaviour
{
    [SerializeField] private Color color;
    private static MaterialPropertyBlock block;
    private static int colorId = Shader.PropertyToID("_BaseColor");

    private MeshRenderer meshRenderer;
    private MeshRenderer MeshRenderer => meshRenderer ?? (meshRenderer = GetComponent<MeshRenderer>());

    private void Awake()
    {
        OnValidate();
    }

    private void OnValidate()
    {
        if (block == null)
        {
            block = new MaterialPropertyBlock();
        }

        block.SetColor(colorId, color);
        MeshRenderer.SetPropertyBlock(block);
    }
}