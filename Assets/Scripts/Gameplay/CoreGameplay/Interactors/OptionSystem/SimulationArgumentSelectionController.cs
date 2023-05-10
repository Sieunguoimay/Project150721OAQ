using Framework.DependencyInversion;
using Gameplay.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SimulationArgumentSelectionController : ScriptableEntity
{
    [SerializeField] private BaseSelection[] supporedSelections;
    [SerializeField, UseMemberValueAsLabel(nameof(FollowSelectionSequence.CardType))]
    private FollowSelectionSequence[] followSelectionSequences;

    public List<SimulationArgument> ArgumentList { get; } = new();
    public List<ISelection> SelectionList { get; } = new();

    private ISelection _currentSelection;
    private bool _firstSelection;

    public event Action<SimulationArgumentSelectionController> OnArgumentsSelectionResult;
    public IEnumerable<ISelection> SupportedSelections => supporedSelections;

    public void AddArgument(SimulationArgumentType type, int selectedValue)
    {
        AddArgument(new SimulationArgument { argumentType = type, selectedValue = selectedValue });
    }

    public void AddArgument(SimulationArgument argument)
    {
        ArgumentList.Add(argument);
    }
    public void ClearArguments()
    {
        ArgumentList.Clear();
    }
    public void StartSelectionSequence()
    {
        _firstSelection = true;
        SelectionList.Add(GetSelectionByArgumentType(SimulationArgumentType.Card));
        ClearArguments();
        RunNextSelection();
    }

    private void RunNextSelection()
    {
        if (SelectionList.Count > 0)
        {
            _currentSelection = SelectionList.First();
            SelectionList.Remove(_currentSelection);
            _currentSelection.OnSelectionResult -= OnSelectionResult;
            _currentSelection.OnSelectionResult += OnSelectionResult;
            _currentSelection.StartSelection(this);
        }
        else
        {
            OnSelctionSequenceEmpty();
        }
    }

    private void OnSelectionResult(SimulationArgument argument)
    {
        _currentSelection.OnSelectionResult -= OnSelectionResult;
        if (_firstSelection)
        {
            _firstSelection = false;
            if (_currentSelection.ArgumentType == SimulationArgumentType.Card)
            {
                var sequence = GetFollowSequenceOfCard(argument.selectedValue);
                AppendSelectionSequence(sequence);
            }
        }
        AddArgument(argument);
        RunNextSelection();
    }

    private void OnSelctionSequenceEmpty()
    {
        OnArgumentsSelectionResult?.Invoke(this);
        LogDebugResult();
    }

    private void AppendSelectionSequence(FollowSelectionSequence found)
    {
        if (found != null)
        {
            SelectionList.AddRange(found.sequence.Select(GetSelectionByArgumentType));
        }
    }

    private FollowSelectionSequence GetFollowSequenceOfCard(int selectedIndex)
    {
        var cardType = (CardType)GetSelectionByArgumentType(SimulationArgumentType.Card).GetSelectedData(selectedIndex);
        return followSelectionSequences.FirstOrDefault(s => s.CardType == cardType);

    }
    public ISelection GetSelectionByArgumentType(SimulationArgumentType type)
    {
        return SupportedSelections.FirstOrDefault(s => s.ArgumentType == type);
    }

    [Serializable]
    private class FollowSelectionSequence
    {
        public CardType CardType;
        public SimulationArgumentType[] sequence;
    }
#if UNITY_EDITOR
    [ContextMenu("Test")]
    private void Test()
    {
        StartSelectionSequence();
    }
#endif
    private void LogDebugResult()
    {
        var arr = ArgumentList.Select(a =>
        {
            var s = GetSelectionByArgumentType(a.argumentType);
            var v = s.GetSelectedData(a.selectedValue);
            if (v is Card c)
            {
                return $"({a.argumentType} {c.CardType})";
            }
            return $"({a.argumentType} {v})";
        });
        Debug.Log($"{string.Join(",", arr)}");
    }
}

public class SimulationArgument
{
    public SimulationArgumentType argumentType;
    public int selectedValue;
}

public enum SimulationArgumentType
{
    Card,
    Tile,
    Direction
}