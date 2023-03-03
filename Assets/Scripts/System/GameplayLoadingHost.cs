using System.Collections.Generic;
using System.Linq;

namespace System
{
    public interface IGameplayLoadingHost
    {
        void Register(IGameplayLoadingUnit unit);
        void NotifyUnitLoadingDone(IGameplayLoadingUnit unit);
        void NotifyUnitUnloadingDone(IGameplayLoadingUnit unit);
        event Action<GameplayLoadingHost> LoadingDoneEvent;
        event Action<GameplayLoadingHost> UnloadBeginEvent;
        event Action<GameplayLoadingHost> UnloadingDoneEvent;
    }

    public class GameplayLoadingHost : IGameplayLoadingHost
    {
        private List<InternalLoadingUnit> _units;

        public void Register(IGameplayLoadingUnit unit)
        {
            _units.Add(new InternalLoadingUnit() {Unit = unit, IsLoaded = false, IsUnloaded = false});
        }

        public void ResetLoadingUnit()
        {
            foreach (var unit in _units)
            {
                unit.IsLoaded = false;
                unit.IsUnloaded = false;
            }
        }

        public void NotifyUnitLoadingDone(IGameplayLoadingUnit unit)
        {
            var found = _units.FirstOrDefault(u => u.Unit == unit);
            if (found != null) found.IsLoaded = true;
            if (IsAllUnitsLoaded())
            {
                InvokeLoadingDoneEvent();
            }
        }

        public void NotifyUnitUnloadingDone(IGameplayLoadingUnit unit)
        {
            var found = _units.FirstOrDefault(u => u.Unit == unit);
            if (found != null) found.IsUnloaded = true;
            if (IsAllUnitsUnloaded())
            {
                InvokeUnloadingDoneEvent();
            }
        }

        public void LoadAllUnits()
        {
            foreach (var unit in _units)
            {
                unit.Unit.Load(this);
            }
        }

        public void UnloadAllUnits()
        {
            UnloadBeginEvent?.Invoke(this);
            foreach (var unit in _units)
            {
                unit.Unit.Unload(this);
            }
        }

        private bool IsAllUnitsLoaded()
        {
            return _units.All(u => u.IsLoaded);
        }

        private bool IsAllUnitsUnloaded()
        {
            return _units.All(u => u.IsUnloaded);
        }

        private void InvokeLoadingDoneEvent()
        {
            LoadingDoneEvent?.Invoke(this);
        }

        private void InvokeUnloadingDoneEvent()
        {
            UnloadingDoneEvent?.Invoke(this);
        }

        public event Action<GameplayLoadingHost> LoadingDoneEvent;
        public event Action<GameplayLoadingHost> UnloadBeginEvent;
        public event Action<GameplayLoadingHost> UnloadingDoneEvent;

        private class InternalLoadingUnit
        {
            public IGameplayLoadingUnit Unit;
            public bool IsLoaded;
            public bool IsUnloaded;
        }
    }

    public interface IGameplayLoadingUnit
    {
        void Load(GameplayLoadingHost host);
        void Unload(GameplayLoadingHost host);
    }
}