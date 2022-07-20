using System;
using Interfaces;
using UnityEngine;

namespace Implementations.Transporter
{
    public class PassengerMono : AMonoBehaviourWrapper<IPassenger>
    {
        [SerializeField] private TransporterMono transporterMono;
        [SerializeField] private TransformableMono target;
        [ContextMenu("Test CallOutForTransporter")]
        private void CallOutForTransporter()
        {
            Target.SetTicket(new TransportTicket(new TransportTicket.ConfigData()
            {
                attachPoint = Vector3.up,
                destination = target.Target.GetPosition()
            }));
            transporterMono.Target.Attach(Target);
        }

        public override IPassenger Create()
        {
            return new Passenger(transform);
        }
    } 
}