using UnityEngine;

namespace Interfaces
{
    public interface IPassenger : ITransformable
    {
        void SetTicket(ITransportTicket ticket);
        ITransportTicket GetTicket();
        void OnPickUp(ITransporter transporter);
        void OnReachDestination(ITransporter transporter);
    }

    public interface ITransportTicket
    {
        Vector3 GetAttachPoint();
        Vector3 GetDestination();
    }
}