using System;
using System.Collections.Generic;
using SNM.Bezier;
using UnityEngine;

[CreateAssetMenu(menuName = "Managers/DroneManager")]
public class DroneManager : ScriptableObject
{
    [SerializeField] private ConfigData configData;

    [NonSerialized] private List<Drone> drones = new List<Drone>();
    [NonSerialized] private GameObject container;

    [Serializable]
    public class ConfigData
    {
        [SerializeField] public Drone.ConfigData droneCommonConfig;
    }

    public void Setup(Transform endPoint)
    {
        container = new GameObject(name);
        
        // var d = Instantiate(Main.Instance.PrefabManager.GetPrefab<Drone>(), container.transform).GetComponent<Drone>();
        //
        // d.Setup(configData.droneCommonConfig, endPoint);
        //
        // drones.Add(d);
    }

    public void Loop(float deltaTime)
    {
        // foreach (var drone in drones)
        // {
        //     drone.Loop(deltaTime);
        // }
    }

    public void Cleanup()
    {
        // foreach (var drone in drones)
        // {
        //     drone.Cleanup();
        // }
        //
        // GameObject.Destroy(container);
    }

    // public Drone GetDrone()
    // {
    //     // return drones[0];
    // }
}