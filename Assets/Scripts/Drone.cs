using UnityEditor;
using UnityEngine;

public class Drone
{
    public class StateData
    {
        public GameObject gameObject;
    }

    public class ConfigData
    {
    }

    private ConfigData configData;
    private StateData stateData;

    public Drone(ConfigData configData, GameObject gameObject)
    {
        this.configData = configData;
        stateData = new StateData();
        stateData.gameObject = gameObject;
    }
}