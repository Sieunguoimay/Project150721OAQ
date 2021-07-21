using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Citizen : Prefab
{
    private Mover mover;
    public Mover Mover => mover ?? (mover = new Mover(transform));
}