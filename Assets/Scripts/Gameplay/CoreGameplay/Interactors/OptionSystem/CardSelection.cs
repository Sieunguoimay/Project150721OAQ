using Common;
using Framework.DependencyInversion;
using Gameplay.Cards;
using SNM;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

public interface ISelection
{
    SimulationArgumentType ArgumentType { get; }
    void StartSelection(SimulationArgumentSelectionList selectionController);
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
    public event Action<ISelection> OnSelectingChanged;

    public abstract object GetOptionDataByIndex(int selectedIndex);
    protected override void OnSetupDependencies()
    {
        base.OnSetupDependencies();
        _isSelecting = false;
    }
    public void StartSelection(SimulationArgumentSelectionList selectionController)
    {
        SetSelecting(true);
        OnStartSelection();
    }
    protected abstract void OnStartSelection();
    protected void InvokeOnSelectionResult(SimulationArgument argument)
    {
        if (!_isSelecting) return;
        SetSelecting(false);
        OnSelectionResult?.Invoke(argument);
    }
    private void SetSelecting(bool isSelecting)
    {
        _isSelecting = isSelecting;
        OnSelectingChanged?.Invoke(this);
    }
}

public class CardSelection : BaseSelection
{
    [SerializeField] private CardContainer cardContainer;
    [SerializeField] private bool debug = false;

    public Card SelectedCard { get; private set; }
    public IReadOnlyList<Card> Cards => cardContainer.Cards;

    public event Action<CardSelection> OnSelectedCardChanged;

    protected override void OnSetupDependencies()
    {
        base.OnSetupDependencies();
        foreach (var card in Cards)
        {
            card.OnSelectedChanged -= OnCardSelectedChanged;
            card.OnSelectedChanged += OnCardSelectedChanged;
        }
    }

    protected override void OnTearDownDependencies()
    {
        base.OnTearDownDependencies();
        foreach (var card in Cards)
        {
            card.OnSelectedChanged -= OnCardSelectedChanged;
        }
    }

    private void OnCardSelectedChanged(Card card)
    {
        if (IsSelecting)
        {
            if (card.IsSelected)
            {
                SelectCard(card);
            }
        }
    }

    public override object GetOptionDataByIndex(int optionIndex)
    {
        if (optionIndex == -1) return CardType.None;
        return Cards[optionIndex].CardType;
    }

    protected override void OnStartSelection()
    {
        SelectedCard = null;
        if (debug)
        {
            SelectCard(Cards.GetRandom());
        }
    }

    public void SelectCard(Card card)
    {
        SelectedCard = card;
        OnSelectedCardChanged?.Invoke(this);
        InvokeOnSelectionResult(new SimulationArgument
        {
            argumentType = SimulationArgumentType.Card,
            selectedValue = card != null ? Cards.Select((c, i) => (c, i)).FirstOrDefault(c => c.c == card).i : -1
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
        SelectCard(Cards[0]);
    }
    [ContextMenu("Test 2")]
    private void Test2()
    {
        SelectCard(Cards[1]);
    }
    [ContextMenu("Test 3")]
    private void Test3()
    {
        SelectCard(Cards[2]);
    }
#endif
}