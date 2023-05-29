using Common.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif
using UnityEngine;
using UnityEngine.Events;

public class AnimatorControllerCacher : MonoBehaviour, IAnimationStateEventHandler
{
    [SerializeField] private AnimatorControllerCacheData cacheData;

    public void OnStateEnter(AnimatorStateInfo stateInfo, int layerIndex)
    {
        GetState(stateInfo, layerIndex)?.InvokeOnStateEnter();
        Debug.Log("OnStateEnter");
    }

    public void OnStateExit(AnimatorStateInfo stateInfo, int layerIndex)
    {
        GetState(stateInfo, layerIndex)?.InvokeOnStateExit();
        Debug.Log("OnStateExit");
    }

    private AnimatorControllerCacheData.State GetState(AnimatorStateInfo stateInfo, int layerIndex)
    {
        return cacheData.Layers[layerIndex].GetState(stateInfo.shortNameHash);
    }

#if UNITY_EDITOR

    [ContextMenu(nameof(CacheAnimatorControllerData))]
    private void CacheAnimatorControllerData()
    {
        if (!TryGetComponent<Animator>(out var animator)) return;
        var controller = animator.runtimeAnimatorController as AnimatorController;
        Debug.Log(AssetDatabase.GetAssetPath(controller));
        foreach (var layer in controller.layers)
        {
            foreach (var s in layer.stateMachine.states)
            {
                Debug.Log($"{layer.name} {s.state.name} {AssetDatabase.GetAssetPath(s.state.motion)}");
            }
        }
        foreach (var parameter in controller.parameters)
        {
            Debug.Log($"{parameter.name} {parameter.type}");
        }
        cacheData.CopyFromAnimator(animator);
    }
#endif
}
[Serializable]
public class AnimatorControllerCacheData
{
    [SerializeField] private AnimatorController controllerAsset;
    [SerializeField] private Layer[] layers;
    [SerializeField] private AnimatorControllerParameter[] parameters;

#if UNITY_EDITOR
    public void CopyFromAnimator(Animator animator)
    {
        var controller = animator.runtimeAnimatorController as AnimatorController;
        layers = new Layer[controller.layers.Length];
        for (int i = 0; i < controller.layers.Length; i++)
        {
            layers[i] = new Layer();
            layers[i].CopyFromEditorLayer(controller.layers[i]);
        }
        parameters = controller.parameters;
        controllerAsset = controller;
    }
#endif
    public AnimatorController ControllerAsset => controllerAsset;
    public Layer[] Layers => layers;
    public AnimatorControllerParameter[] Parameters => parameters;

    [Serializable]
    public class Layer
    {
        public string layerName;
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
    public class State
    {
        public string name;
        public int nameHash;
        public Motion motion;
        public float speed;
        public Transition[] transitions;
        public UnityEvent onStateEnter;
        public UnityEvent onStateExit;
        [SerializeField, HideInInspector] private bool isCurrent;

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
        public void InvokeOnStateEnter()
        {
            onStateEnter?.Invoke();
            isCurrent = true;
            Debug.Log("InvokeOnStateEnter");
        }
        public void InvokeOnStateExit()
        {
            onStateExit?.Invoke();
            isCurrent = false;
            Debug.Log("InvokeOnStateExit");
        }
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


#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(State))]
    public class StatePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //base.OnGUI(position, property, label);
            var isCurrent = property.FindPropertyRelative("isCurrent").boolValue;
            EditorGUI.PropertyField(position, property, new GUIContent($"- {label.text}{(isCurrent ? " (Current)" : "")}"), true);
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property);// property.isExpanded?EditorGUI.GetPropertyHeight(property):base.GetPropertyHeight(property, label);
        }
    }
#endif
}
