using Common.UnityExtend.Animation;
using UnityEngine;

public class AnimatorStringInvoker : MonoBehaviour
{
    [SerializeField] private AnimatorStringsContainer animatorStringContainer;
    [SerializeField, StringSelectorByLocalID(nameof(animatorStringContainer))] private int selectedLocalID;

    private IStringsContainerProvider StringContainerProvider => animatorStringContainer;
    private Animator Animator => animatorStringContainer.Animator;

    public string GetValue()
    {
        if (StringContainerProvider.StringContainer.TryGetValue(selectedLocalID, out var result))
        {
            return result;
        }
        return null;
    }

    [ContextMenu(nameof(SetTrigger))]
    public void SetTrigger()
    {
        Animator.SetTrigger(GetValue());
    }

    [ContextMenu(nameof(Play))]
    public void Play()
    {
        Animator.Play(GetValue());
    }

    public void SetFloat(float value)
    {
        Animator.SetFloat(GetValue(), value);
    }

    public void SetBool(bool value)
    {
        Animator.SetBool(GetValue(), value);
    }
}
