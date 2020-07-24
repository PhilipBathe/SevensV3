﻿using System;
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

        var canvas = GameObject.Find("Canvas");
        var enemiesPanel = GameObject.Find("Enemies");

        var player = Instantiate(PlayerPrefab, Vector2.zero, Quaternion.identity) as GameObject;
        players.Add(player);
        player.transform.SetParent(canvas.transform, false);

        for (int i = 0; i < NumberOfEnemies; i++)
        {
            var enemy = Instantiate(EnemyPrefab, Vector2.zero, Quaternion.identity) as GameObject;
            enemy.transform.SetParent(enemiesPanel.transform, false);
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

    int activePlayerIndex = 0;

    public void NextPlayer(bool newGame = false)
    {
        if(newGame == true)
        {
            finishedPlayers = new List<int>();

            for(int i = 0; i < players.Count; i++)
            {
                var player = players[i].GetComponent<Player>();
                player.IsActive = false;
                if(player.HasSevenOfDiamonds()) 
                {
                    //Debug.Log("Found 7 of diamonds");
                    player.IsActive = true;
                    activePlayerIndex = i;
                }
            }
            return;
        }
        else
        {
            var player = players[activePlayerIndex].GetComponent<Player>();
            player.IsActive = false;

            if(players.Count <= finishedPlayers.Count)
            {
                Debug.Log("All done!");
                return;
            }

            activePlayerIndex++;
            if(activePlayerIndex >= players.Count)
            {
                activePlayerIndex = 0;
            }

            while(finishedPlayers.Contains(activePlayerIndex))
            {
                activePlayerIndex++;
                if(activePlayerIndex >= players.Count)
                {
                    activePlayerIndex = 0;
                }
            }

            player = players[activePlayerIndex].GetComponent<Player>();
            player.IsActive = true;
        }

    }

    List<int> finishedPlayers = new List<int>();

    public int OutOfCards()
    {
        finishedPlayers.Add(activePlayerIndex);
        return finishedPlayers.Count;
    }
}
