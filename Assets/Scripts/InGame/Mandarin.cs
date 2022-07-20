using System.Collections;
using System.Collections.Generic;
using Implementations.Transporter;
using Interfaces;
using UnityEngine;

[SelectionBase]
public class Mandarin : Piece
{
    private IPassenger _passenger;
    public IPassenger Passenger => _passenger ?? (_passenger = new Passenger(transform));
}