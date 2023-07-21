using Gameplay.Visual.Views;
using UnityEngine;

public class InteractSystemRepresenter : ScriptableObject
{
    public InteractSystem Author { get; private set; }
    public void SetAuthor(InteractSystem author)
    {
        Author = author;
    }
}
