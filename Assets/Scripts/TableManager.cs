using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableManager : MonoBehaviour
{
    public GameObject PlayerPrefab;
    public GameObject EnemyPrefab;
    public int NumberOfEnemies = 3;


    private List<GameObject> players;

    void Start()
    {
        setupPlayers();
    }

    private void setupPlayers()
    {
        players = new List<GameObject>();

        var canvas = GameObject.Find("Canvas").transform;

        var player = Instantiate(PlayerPrefab, new Vector3(-2, -3, 0), Quaternion.identity) as GameObject;
        players.Add(player);

        for (int i = 0; i < NumberOfEnemies; i++)
        {
            var enemy = Instantiate(EnemyPrefab, new Vector3((5*(1-i))-1,4), Quaternion.identity) as GameObject;
            players.Add(enemy);
        }
    }

    public List<GameObject> GetPlayers()
    {
        return players;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
