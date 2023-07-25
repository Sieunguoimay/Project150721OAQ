using SaveData;
using UnityEngine;

public class TestSaveData : SaveDataSO
{
    [SerializeField] private string test;
    public string Test => test;
}