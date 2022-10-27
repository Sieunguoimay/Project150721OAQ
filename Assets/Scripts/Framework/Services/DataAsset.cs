using System;
using UnityEngine;

namespace Framework.Services
{
    public class DataAsset : ScriptableObject
    {
        [field: SerializeField] public string Id { get; private set; }
    }
}