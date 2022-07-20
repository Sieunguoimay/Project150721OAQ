using System;
using Interfaces;
using UnityEngine;

namespace Implementations.Transporter
{
    public class Passenger : Transformable, IPassenger
    {
        private ITransportTicket _ticket;

        public Passenger(Transform transform) : base(transform)
        {
        }

        public void SetTicket(ITransportTicket ticket)
        {
            _ticket = ticket;
        }

        public ITransportTicket GetTicket()
        {
            return _ticket;
        }

        public void OnPickUp(ITransporter transporter)
        {
        }

        public void OnReachDestination(ITransporter transporter)
        {
        }
    }

    public class TransportTicket : ITransportTicket
    {
        [Serializable]
        public class ConfigData
        {
            public Vector3 attachPoint;
            public Vector3 destination;
        }

        private readonly ConfigData _config;

        public TransportTicket(ConfigData config)
        {
            _config = config;
        }

        public Vector3 GetAttachPoint()
        {
            return _config.attachPoint;
        }

        public Vector3 GetDestination()
        {
            return _config.destination;
        }
    }
}