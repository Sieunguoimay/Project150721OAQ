using System;
using System.Collections.Generic;
using UnityEngine;

public class DroneManager
{
    private List<Drone> drones;

    [Serializable]
    public class ConfigData
    {
         
    }
    
    public DroneManager()
    {
        drones = new List<Drone>();
    }
}