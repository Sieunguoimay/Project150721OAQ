using Common;
using Framework.DependencyInversion;
using Gameplay.Cards;
using SNM;
using System;
using UnityEngine;

public interface ISelection
{
    SimulationArgumentType ArgumentType { get; }
    void StartSelection(SimulationArgumentSelectionController selectionController);
    object GetSelectedData(int selectedIndex);
    event Action<SimulationArgument> OnSelectionResult;
}
public abstract class BaseSelection : ScriptableEntity, ISelection
{
    [SerializeField] private SimulationArgumentType argumentType;

    private bool _onDuty;

    public SimulationArgumentType ArgumentType => argumentType;

    public event Action<SimulationArgument> OnSelectionResult;

    public abstract object GetSelectedData(int selectedIndex);

    public void StartSelection(SimulationArgumentSelectionController selectionController)
    {
        _onDuty = true;
        OnStartSelection();
    }
    protected abstract void OnStartSelection();
    protected void InvokeOnSelectionResult(SimulationArgument argument)
    {
        if (!_onDuty) return;
        OnSelectionResult?.Invoke(argument);
    }
}

public class CardSelection : BaseSelection
{
    [SerializeField] private Card[] cards;
    [SerializeField] private bool debug = false;

    public override object GetSelectedData(int selectedIndex)
    {
        if (selectedIndex == -1) return CardType.None;
        return cards[selectedIndex].CardType;
    }

    protected override void OnStartSelection()
    {
        if (debug) return;

        if (Application.isPlaying)
        {
            PublicExecutor.Instance.Delay(1, () =>
            {
                var selected = cards.GetRandom();
                OnCardSelectionResult(selected);
            });
        }
        else
        {
            var selected = cards.GetRandom();
            OnCardSelectionResult(selected);
        }
    }

    private void OnCardSelectionResult(Card card)
    {
        InvokeOnSelectionResult(new SimulationArgument
        {
            argumentType = SimulationArgumentType.Card,
            selectedValue = card != null ? Array.IndexOf(cards, card) : -1
        });
    }

#if UNITY_EDITOR
    [ContextMenu("Test 0")]
    private void Test()
    {
        OnCardSelectionResult(null);
    }
    [ContextMenu("Test 1")]
    private void Test1()
    {
        OnCardSelectionResult(cards[0]);
    }
    [ContextMenu("Test 2")]
    private void Test2()
    {
        OnCardSelectionResult(cards[1]);
    }
    [ContextMenu("Test 3")]
    private void Test3()
    {
        OnCardSelectionResult(cards[2]);
    }
#endif
}