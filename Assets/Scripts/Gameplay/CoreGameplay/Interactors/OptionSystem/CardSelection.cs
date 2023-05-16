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
    object GetOptionDataByIndex(int selectedIndex);
    event Action<SimulationArgument> OnSelectionResult;
}

public abstract class BaseSelection : ScriptableEntity, ISelection
{
    [SerializeField] private SimulationArgumentType argumentType;

    private bool _isSelecting;
    public bool IsSelecting => _isSelecting;

    public SimulationArgumentType ArgumentType => argumentType;

    public event Action<SimulationArgument> OnSelectionResult;

    public abstract object GetOptionDataByIndex(int selectedIndex);

    public void StartSelection(SimulationArgumentSelectionController selectionController)
    {
        _isSelecting = true;
        OnStartSelection();
    }
    protected abstract void OnStartSelection();
    protected void InvokeOnSelectionResult(SimulationArgument argument)
    {
        if (!_isSelecting) return;
        OnSelectionResult?.Invoke(argument);
    }
}

public class CardSelection : BaseSelection
{
    [SerializeField] private Card[] cards;
    [SerializeField] private bool debug = false;

    public Card SelectedCard { get; private set; }
    public event Action<CardSelection> OnSelectedCardChanged;

    public override object GetOptionDataByIndex(int optionIndex)
    {
        if (optionIndex == -1) return CardType.None;
        return cards[optionIndex].CardType;
    }

    protected override void OnStartSelection()
    {
        SelectedCard = null;
        //if (debug) return;

        //if (Application.isPlaying)
        //{
        //    PublicExecutor.Instance.Delay(1, () =>
        //    {
        //        var selected = cards.GetRandom();
        //        SelectCard(selected);
        //    });
        //}
        //else
        //{
        //    var selected = cards.GetRandom();
        //    SelectCard(selected);
        //}
    }

    public void SelectCard(Card card)
    {
        SelectedCard = card;
        OnSelectedCardChanged?.Invoke(this);
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
        SelectCard(null);
    }
    [ContextMenu("Test 1")]
    private void Test1()
    {
        SelectCard(cards[0]);
    }
    [ContextMenu("Test 2")]
    private void Test2()
    {
        SelectCard(cards[1]);
    }
    [ContextMenu("Test 3")]
    private void Test3()
    {
        SelectCard(cards[2]);
    }
#endif
}