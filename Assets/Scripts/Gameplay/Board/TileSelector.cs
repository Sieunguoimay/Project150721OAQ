using System;
using System.Collections.Generic;
using System.Linq;
using Common.ResolveSystem;
using Gameplay.Piece;
using SNM;
using UnityEngine;

namespace Gameplay.Board
{
    public interface ISelectorTarget
    {
        IEnumerable<CitizenToTileSelectorAdaptor> SelectionAdaptors { get; }
        Vector3 DisplayPos { get; }
    }

    public class TileSelector : MonoBehaviour, IInjectable
    {
        [SerializeField] private MeshBoundsClicker left;
        [SerializeField] private MeshBoundsClicker right;

        private Gameplay.Board.Board.TileGroup _tileGroup;
        private ISelectorTarget _selected;
        private readonly List<ISelectionAdaptor> _selectionAdaptors = new();

        public Action<ISelectorTarget, bool> OnDone = delegate { };
        public event Action<bool> OnTouched;

        public void Inject(IResolver resolver)
        {
        }

        private void Awake()
        {
            gameObject.SetActive(false);
            left.OnClick += InvokeOnTouchedLeft;
            right.OnClick += InvokeOnTouchedRight;
        }

        private void OnDestroy()
        {
            left.OnClick -= InvokeOnTouchedLeft;
            right.OnClick -= InvokeOnTouchedRight;
        }

        public void ResetAll()
        {
            _selected = null;
            _selectionAdaptors.Clear();
            gameObject.SetActive(false);
        }

        public void Display(Gameplay.Board.Board.TileGroup tileGroup)
        {
            _selected = null;
            _tileGroup = tileGroup;
        }

        public void SelectTile(ISelectorTarget tile)
        {
            var adapters = _selected.SelectionAdaptors;
            
            if (!adapters?.Any()??false) return;

            _selected = tile;

            InvokeDeselect(false);

            foreach (var sa in adapters)
            {
                ((ISelectionAdaptor) sa).OnTileSelected();
                _selectionAdaptors.Add(sa);
            }

            transform.position = tile.DisplayPos + Vector3.up * 0.3f;
            
            var tiles = _tileGroup.Tiles;
            var dir = ((Tile) tiles[^1]).transform.position - ((Tile) tiles[0]).transform.position;
            dir = SNM.Math.Projection(dir, Vector3.up);
            transform.rotation = Quaternion.LookRotation(dir, transform.up);

            gameObject.SetActive(true);
        }

        private void InvokeDeselect(bool success)
        {
            foreach (var sa in _selectionAdaptors)
            {
                sa.OnTileDeselected(success);
            }

            _selectionAdaptors.Clear();
        }

        public void ChooseDirection(bool forward)
        {
            InvokeDeselect(true);

            _tileGroup = null;

            OnDone?.Invoke(_selected, forward);
            gameObject.SetActive(false);
        }

        private void InvokeOnTouchedRight()
        {
            OnTouched?.Invoke(true);
        }

        private void InvokeOnTouchedLeft()
        {
            OnTouched?.Invoke(false);
        }

        public interface ISelectionAdaptor
        {
            void OnTileSelected();
            void OnTileDeselected(bool success);
        }
    }
}