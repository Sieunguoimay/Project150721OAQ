using System;
using Interfaces;
using UnityEngine;

namespace Implementations.Transporter
{
    public class MoverMono : AMonoBehaviourWrapper<IMover>, IMoverListener
    {
        [SerializeField] private Mover.ConfigData config;
        public override IMover Create() => new Mover(config);

        private void Awake()
        {
            Target.Attach(this);
        }

        private void Update()
        {
            Target.SetPosition(transform.position);
            Target.Loop(Time.deltaTime);
            transform.position = Target.GetPosition();
        }

        [ContextMenu("Test Move")]
        private void MoveRandom()
        {
            Target.MoveTo(UnityEngine.Random.insideUnitSphere * 5f);
        }

        public void OnReachTarget(IMover mover)
        {
            Debug.Log("Reached target !!");
        }
    }
}