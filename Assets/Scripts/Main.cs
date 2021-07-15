using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    public Board Board { get; private set; }

    void Start()
    {
        Board = Instantiate(PrefabManager.Instance.BoardPrefab);
        Board.Setup();
    }

    void Update()
    {
    }
}