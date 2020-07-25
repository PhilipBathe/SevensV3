﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableManager : MonoBehaviour
{
    public GameObject PlayerPrefab;
    public GameObject EnemyPrefab;
    private int numberOfEnemies = 3;
    private float wineLevel = 0;

    private GameObject enemiesPanel;


    private List<GameObject> players = new List<GameObject>();

    void Start()
    {
        enemiesPanel = GameObject.Find("Enemies");
        setupPlayers();
        setupEnemies();
    }

    private void setupPlayers()
    {
        var canvas = GameObject.Find("Canvas");

        var player = Instantiate(PlayerPrefab, Vector2.zero, Quaternion.identity) as GameObject;
        players.Add(player);
        player.transform.SetParent(canvas.transform, false);
    }

    private void setupEnemies()
    {
        for (int i = 0; i < numberOfEnemies; i++)
        {
            var enemy = Instantiate(EnemyPrefab, Vector2.zero, Quaternion.identity) as GameObject;
            //enemy.GetComponent<Player>().SetName($"AI Player {i + 1}");
            enemy.GetComponent<Player>().PickRandomName();
            enemy.GetComponent<Player>().PickRandomColor();
            enemy.GetComponent<Player>().AIWineLevel = wineLevel;
            enemy.transform.SetParent(enemiesPanel.transform, false);
            players.Add(enemy);
        }
    }

    public void ChangeNumberOfEnemies(float numberOfPlayers)
    {
        int newNumberOfEnemies = (int)numberOfPlayers - 1;

        if(numberOfEnemies != newNumberOfEnemies)
        {
            numberOfEnemies = newNumberOfEnemies;
            clearEnemies();
            setupEnemies();
        }
    }

    public void SetWineLevelOfEnemies(float newWineLevel)
    {
        if(wineLevel != newWineLevel)
        {
            wineLevel = newWineLevel;

            foreach(Transform enemy in enemiesPanel.transform)
            {
               enemy.gameObject.GetComponent<Player>().AIWineLevel = wineLevel;
            }
        }
    }

    private void clearEnemies()
    {
        Debug.Log("clear enemies");

        foreach(Transform enemy in enemiesPanel.transform)
        {
            players.Remove(enemy.gameObject);
            Destroy(enemy.gameObject);
        }
        enemiesPanel.transform.DetachChildren();
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
                player.IsActivePlayer = false;
                if(player.HasSevenOfDiamonds()) 
                {
                    //Debug.Log("Found 7 of diamonds");
                    activePlayerIndex = i;
                    player.StartTurn();
                }
            }
            return;
        }
        else
        {
            var player = players[activePlayerIndex].GetComponent<Player>();
            player.IsActivePlayer = false;

            if(players.Count <= finishedPlayers.Count)
            {
                Debug.Log("All done!");
                GameObject.Find("DealButton").GetComponent<DealCards>().ShowNextGamePanel();
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
            player.StartTurn();
        }

    }

    List<int> finishedPlayers = new List<int>();

    public int OutOfCards()
    {
        finishedPlayers.Add(activePlayerIndex);
        return finishedPlayers.Count;
    }
}
