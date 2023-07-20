using Gameplay.CoreGameplay.Interactors;
using Gameplay.Visual.Views;
using UnityEngine;

public class BoardVisualGeneratorRepresenter : ScriptableObject
{
    public BoardVisualGenerator Author { get ; private set; }

    public void SetAuthor(BoardVisualGenerator author)
    {
        Author = author;
    }
    public void RefreshVisual(RefreshData refreshData)
    {
        Author.GenerateBoardVisual(refreshData);
    }
}