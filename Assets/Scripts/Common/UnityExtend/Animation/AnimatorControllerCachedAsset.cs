using System.Collections.Generic;
using System.Linq;
using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif
using UnityEngine;
using Common.UnityExtend.Attribute;
using Sieunguoimay.Attribute;

public class AnimatorControllerCachedAsset : ScriptableObject
{
#if UNITY_EDITOR
    [SerializeField] private AnimatorController controllerAsset;
#endif
    [SerializeField, Disable] private Layer[] layers;
    [SerializeField, Disable] private Parameter[] parameters;

#if UNITY_EDITOR
    public AnimatorController ControllerAsset => controllerAsset;
#endif
    public Layer[] Layers => layers;
    public Parameter[] Parameters => parameters;

#if UNITY_EDITOR

    [ContextMenu(nameof(CacheAnimatorControllerData))]
    public void CacheAnimatorControllerData()
    {
        CopyFromAnimatorController(controllerAsset);
    }

    public void CopyFromAnimatorController(AnimatorController controller)
    {
        layers = new Layer[controller.layers.Length];
        for (int i = 0; i < controller.layers.Length; i++)
        {
            layers[i] = new Layer();
            layers[i].CopyFromEditorLayer(controller.layers[i]);
        }
        parameters = new Parameter[controller.parameters.Length];
        for (var i = 0; i < controller.parameters.Length; i++)
        {
            var param = controller.parameters[i];
            parameters[i] = new Parameter
            {
                name = param.name,
                nameHash = param.nameHash,
                type = param.type,
                defaultBool = param.defaultBool,
                defaultFloat = param.defaultFloat,
                defaultInt = param.defaultInt
            };
        }
    }
#endif

    [Serializable]
    public class Layer
    {
        [HideInInspector] public string layerName;
        public State[] states;

#if UNITY_EDITOR
        public void CopyFromEditorLayer(AnimatorControllerLayer layer)
        {
            layerName = layer.name;
            states = new State[layer.stateMachine.states.Length];
            for (var i = 0; i < layer.stateMachine.states.Length; i++)
            {
                states[i] = new State();
                states[i].CopyFromEditorState(layer.stateMachine.states[i].state);
            }
        }
#endif
        private Dictionary<int, State> _stateDict;
        public State GetState(int nameHash)
        {
            _stateDict ??= CreateStateDict();
            if (_stateDict.TryGetValue(nameHash, out var state))
            {
                return state;
            }
            return null;
        }
        private Dictionary<int, State> CreateStateDict()
        {
            return states.ToDictionary(s => s.nameHash, s => s);
        }
    }
    [Serializable]
    public class Parameter
    {
        [HideInInspector] public string name;
        [HideInInspector] public int nameHash;
        public AnimatorControllerParameterType type;

        [ShowIf(nameof(type), AnimatorControllerParameterType.Int)]
        public int defaultInt;

        [ShowIf(nameof(type),
            AnimatorControllerParameterType.Bool,
            AnimatorControllerParameterType.Trigger)]
        public bool defaultBool;

        [ShowIf(nameof(type), AnimatorControllerParameterType.Float)]
        public float defaultFloat;
    }

    [Serializable]
    public class State
    {
        [HideInInspector] public string name;
        //[HideInInspector]
        public int nameHash;
        public Motion motion;
        public float speed;
        public Transition[] transitions;

#if UNITY_EDITOR
        public void CopyFromEditorState(AnimatorState editorState)
        {
            name = editorState.name;
            nameHash = editorState.nameHash;
            speed = editorState.speed;
            motion = editorState.motion;
            transitions = new Transition[editorState.transitions.Length];
            for (int i = 0; i < editorState.transitions.Length; i++)
            {
                transitions[i] = new Transition();
                transitions[i].CopyFromEditorTransition(editorState.transitions[i]);
            }
        }
#endif
    }

    [Serializable]
    public class Transition
    {
        public string destinationState;
#if UNITY_EDITOR
        public void CopyFromEditorTransition(AnimatorStateTransition transition)
        {
            destinationState = transition.destinationState.name;
        }
#endif
    }

}
#if UNITY_EDITOR
[CustomEditor(typeof(AnimatorControllerCachedAsset))]
public class AnimatorControllerCachedAssetEditor : Editor
{
    private AnimatorControllerCachedAsset _asset = null;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        _asset = _asset != null ? _asset : target as AnimatorControllerCachedAsset;

        var cacheClicked = GUILayout.Button($"Cache {_asset.ControllerAsset.name}");

        if (cacheClicked)
        {
            _asset.CacheAnimatorControllerData();
        }
    }
}
#endif
