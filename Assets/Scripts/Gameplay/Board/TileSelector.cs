using System;
using System.Collections.Generic;
using System.Linq;
using Common.ResolveSystem;
using Gameplay.Piece;
using SNM;
using UnityEngine;

namespace Gameplay.Board
{
    // public interface ISelectorTarget
    // {
    //     IEnumerable<CitizenToTileSelectorAdaptor> GetSelectionAdaptors();
    //     Vector3 DisplayPos { get; }
    // }
    //
    // [Obsolete]
    // public class TileSelector : MonoBehaviour, IInjectable
    // {
    //     [SerializeField] private MeshBoundsClicker left;
    //     [SerializeField] private MeshBoundsClicker right;
    //
    //     private ISelectorTarget[] _options;
    //     private ISelectorTarget _selected;
    //     private IEnumerable<ISelectionAdaptor> _selectionAdaptors;
    //
    //     public event Action<ISelectorTarget, bool> ChooseDirectionResult;
    //     public event Action<bool> DirectionTouched;
    //
    //     public void Bind(IResolver resolver)
    //     {
    //         resolver.Bind(this);
    //     }
    //
    //     public void Setup(IResolver resolver)
    //     {
    //         gameObject.SetActive(false);
    //
    //         left.Clicked.AddListener(InvokeOnTouchedLeft);
    //         right.Clicked.AddListener(InvokeOnTouchedRight);
    //     }
    //
    //     public void TearDown()
    //     {
    //         left.Clicked.RemoveListener(InvokeOnTouchedLeft);
    //         right.Clicked.RemoveListener(InvokeOnTouchedRight);
    //     }
    //
    //     public void Unbind(IResolver resolver)
    //     {
    //         resolver.Unbind(this);
    //     }
    //
    //     public void ResetAll()
    //     {
    //         _options = null;
    //         _selected = null;
    //         _selectionAdaptors = null;
    //         gameObject.SetActive(false);
    //     }
    //
    //     public void Display(ISelectorTarget[] options)
    //     {
    //         _selected = null;
    //         _options = options;
    //     }
    //
    //     public void SelectTile(ISelectorTarget selected)
    //     {
    //         _selected = selected;
    //
    //         if (_selectionAdaptors != null)
    //         {
    //             InvokeDeselect(false);
    //         }
    //
    //         _selectionAdaptors = _selected.GetSelectionAdaptors();
    //
    //         if (!_selectionAdaptors?.Any() ?? false) return;
    //
    //         foreach (var sa in _selectionAdaptors)
    //         {
    //             sa.OnTileSelected();
    //         }
    //
    //         var dir = _options[^1].DisplayPos - _options[0].DisplayPos;
    //         dir = SNM.Math.Projection(dir, Vector3.up);
    //
    //         transform.position = _selected.DisplayPos + Vector3.up * 0.3f;
    //         transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
    //
    //         gameObject.SetActive(true);
    //     }
    //
    //     private void InvokeDeselect(bool success)
    //     {
    //         foreach (var sa in _selectionAdaptors)
    //         {
    //             sa.OnTileDeselected(success);
    //         }
    //
    //         _selectionAdaptors = null;
    //     }
    //
    //     public void ChooseDirection(bool forward)
    //     {
    //         InvokeDeselect(true);
    //         _options = null;
    //
    //         ChooseDirectionResult?.Invoke(_selected, forward);
    //         gameObject.SetActive(false);
    //     }
    //
    //     private void InvokeOnTouchedRight()
    //     {
    //         DirectionTouched?.Invoke(true);
    //     }
    //
    //     private void InvokeOnTouchedLeft()
    //     {
    //         DirectionTouched?.Invoke(false);
    //     }
    // }

    public interface ISelectionAdaptor
    {
        void OnTileSelected();
        void OnTileDeselected(bool success);
    }
}