using System;
using UnityEditor.Animations;
using UnityEngine;

public interface IReserializeListener
{
    void OnSerialize();
}

[Serializable]
public class AssetNameSelector : IReserializeListener
{
    [SerializeField] private UnityEngine.Object mainAsset;
    [SerializeField] private UnityEngine.Object selectedObject;
    [SerializeField] private string selectedString;
    [SerializeField] private AnimatorController animatorController;

    [SerializeField] string a;
    

    public string GetValue()
    {
        return selectedString;
    }

    public void OnSerialize()
    {
        selectedString = selectedObject.name;
        Animator animator = null;
        var controller = animator.runtimeAnimatorController as AnimatorController;
        //controller.
    }
}
