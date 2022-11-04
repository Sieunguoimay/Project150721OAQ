﻿using Common.UnityExtend.PostProcessing;
using SNM;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Dialog
{
    public class LayerToggle : MonoBehaviour
    {
        [SerializeField, LayerSelector] private string selectedLayer;
        [SerializeField, LayerSelector] private string defaultLayer;

        public void ToggleLayer(GameObject target, bool toggle, bool children = true)
        {
            if (children)
            {
                var all = target.GetComponentsInChildren<Transform>();
                foreach (var child in all)
                {
                    child.gameObject.layer = LayerMask.NameToLayer(toggle ? selectedLayer : defaultLayer);
                }
            }
            else
            {
                target.layer = LayerMask.NameToLayer(toggle ? selectedLayer : defaultLayer);
            }
        }
    }

    public class Dialog : LayerToggle
    {
        [field: SerializeField] public UnityEvent<Transform> OnShow { get; private set; }
        public Transform CurrentTarget { get; private set; }

        public void Show(Transform target)
        {
            if (CurrentTarget != null && CurrentTarget != target)
            {
                ToggleLayer(CurrentTarget.gameObject, false);
            }

            CurrentTarget = target;

            ToggleLayer(CurrentTarget.gameObject, true);

            transform.position = target.position;
            OnShow?.Invoke(target);
        }

        public void HideTarget()
        {
            // _currentTarget.gameObject.SetActive(false);
        }

        public void Hide()
        {
            if (CurrentTarget != null)
            {
                ToggleLayer(CurrentTarget.gameObject, false);
            }

            CurrentTarget = null;
        }
    }
}