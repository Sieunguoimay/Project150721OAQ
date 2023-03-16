using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Common.UnityExtend.Reflection.Tools
{
    public class RuntimeObjectExposeComponent : MonoBehaviour
    {
        [SerializeField] private Object targetObject;
        public IReadOnlyList<RuntimeObjectExpose.ObjectExposedItem> DisplayItems;

        [ContextMenu("Expose")]
        public void Expose()
        {
            DisplayItems = new RuntimeObjectExpose().ExposeObject(targetObject);
            foreach (var displayItem in DisplayItems)
            {
                Debug.Log($"{displayItem.FieldName} {displayItem.DisplayValue}");
            }
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(RuntimeObjectExposeComponent))]
    public class RuntimeObjectExposeComponentEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var component = target as RuntimeObjectExposeComponent;
            if (component != null && component.DisplayItems != null) RuntimeObjectExpose.DrawExposedItems(component.DisplayItems);
        }


    }
#endif
}