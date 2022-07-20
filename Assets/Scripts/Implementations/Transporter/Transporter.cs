using System;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;

namespace Implementations.Transporter
{
    public class Transporter : ITransporter, IMoverListener
    {
        private readonly IMover _mover;
        private readonly ConfigData _config;
        private readonly List<IPassenger> _passengers = new List<IPassenger>();

        private IPassenger _currentPassenger;
        private bool _carryingPassenger;

        [Serializable]
        public class ConfigData
        {
            public Mover.ConfigData moverConfig;
            public Vector3 station;
            public IMovingStyle MovingStyle = new StraightMoving();
        }

        public Transporter(ConfigData config)
        {
            _config = config;
            _mover = new Mover(_config.moverConfig);
            _mover.SetMovingStyle(_config.MovingStyle);
            _mover.Attach(this);
        }

        #region ITransporter

        #region ITransformable

        public void SetPosition(Vector3 position) => _mover.SetPosition(position);
        public Vector3 GetPosition() => _mover.GetPosition();
        public void SetRotation(Quaternion rotation) => _mover.SetRotation(rotation);
        public Quaternion GetRotation() => _mover.GetRotation();

        #endregion ITransformable

        public void Loop(float deltaTime)
        {
            SelectPassenger();
            _mover.Loop(deltaTime);
            if (_currentPassenger != null && _carryingPassenger && _passengers.Count > 1)
            {
                _currentPassenger.SetPosition(GetPosition() - _currentPassenger.GetTicket().GetAttachPoint());
            }
        }

        private void SelectPassenger()
        {
            if (_currentPassenger == null)
            {
                if (_passengers.Count <= 0) return;
                _carryingPassenger = false;
                if (_passengers.Count > 1)
                {
                    _currentPassenger = _passengers[1];
                    _mover.MoveTo(_currentPassenger.GetPosition() + _currentPassenger.GetTicket().GetAttachPoint());
                }
                else
                {
                    _currentPassenger = _passengers[0];
                    _mover.MoveTo(_config.station);
                }
            }
        }

        #endregion ITransporter

        #region IMoverListener

        public void OnReachTarget(IMover mover)
        {
            if (_currentPassenger != null)
            {
                if (!_carryingPassenger)
                {
                    var ticket = _currentPassenger.GetTicket();

                    _carryingPassenger = true;
                    _currentPassenger.OnPickUp(this);
                    _mover.MoveTo(ticket.GetDestination() + ticket.GetAttachPoint());
                }
                else
                {
                    _currentPassenger.OnReachDestination(this);
                    Detach(_currentPassenger);
                    _currentPassenger = null;
                }
            }
        }

        #endregion IMoverListener

        #region IItemHolder<ITranformable>

        public void Attach(IPassenger item)
        {
            if (_passengers.Count == 0)
            {
                var defaultPassenger = new Passenger(null);
                var defaultTicket = new TransportTicket(new TransportTicket.ConfigData()
                {
                    destination = _config.station,
                    attachPoint = Vector3.zero
                });
                defaultPassenger.SetTicket(defaultTicket);
                _passengers.Add(defaultPassenger);
            }
 
            _passengers.Add(item);
        }

        public void Detach(IPassenger item)
        {
            _passengers.Remove(item);
        }

        public IEnumerable<IPassenger> GetItems()
        {
            return _passengers; 
        }

        #endregion IItemHolder<ITranformable>
    }
}