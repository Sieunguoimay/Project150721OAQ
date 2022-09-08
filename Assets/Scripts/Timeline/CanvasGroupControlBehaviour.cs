using System;
using Common.Attribute;
using UnityEngine;
using UnityEngine.Playables;

namespace Timeline
{
    [Serializable]
    public class CanvasGroupControlBehaviour : PlayableBehaviour
    {
        public float alpha = 1f;
        public bool interactable = true;
    }
}