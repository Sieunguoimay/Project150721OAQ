using System;
using UnityEngine;

[DisallowMultipleComponent]
public abstract class MasterComponent : MonoBehaviour
{
    [SerializeField] private string idExtension;
    public Type MasterType => GetType();
    public string UniqueID => $"{MasterType.Name}{idExtension}";

}