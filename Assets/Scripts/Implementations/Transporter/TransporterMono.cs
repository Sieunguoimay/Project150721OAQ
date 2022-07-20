using System;
using Interfaces;
using UnityEngine;

namespace Implementations.Transporter
{
    public class TransporterMono : AMonoBehaviourWrapper<ITransporter>
    {
        [SerializeField] private Transporter.ConfigData config;

        private void Awake()
        {
            config.station = transform.position;
        }

        private void Update()
        {
            Target.SetPosition(transform.position);
            Target.Loop(Time.deltaTime);
            transform.position = Target.GetPosition();
        }

        public override ITransporter Create()
        {
            return new Transporter(config);
        }
    }
}