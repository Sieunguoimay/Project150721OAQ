using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class Citizen : Piece
{
    [SerializeField] private Boid.ConfigData boidConfigData = new Boid.ConfigData()
    {
        maxSpeed = 3f,
        maxAcceleration = 10f,
        arriveDistance = 1f,
        spacing = 0.3f
    };

    public void JumpingMoveTo(Vector3 target)
    {
        var newBoid = new JumpingBoid(
            boidConfigData,
            new Boid.InputData()
            {
                target = target,
                transform = transform
            }, null);
        PieceActor.Add(newBoid);
    }
}