using System.Collections.Generic;
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
}