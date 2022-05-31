
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Map : MonoBehaviour
{
    public static Map inst;

    [HideInInspector]
    public int[,] map;



    void Awake()
    {
        inst = this;
    }

    private void Start()
    {
        map = GetComponent<MapGenerator>().map;
    }
}
