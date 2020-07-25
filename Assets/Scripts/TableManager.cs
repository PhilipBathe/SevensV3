using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
        firstNames = new string[] {"Alberto", "Ali", "Andrew", "Alice", "Art", "Ant", "Amy", "Alesha", "Anjie", "Archie", "Arry", "Alex", "Angel", "Axl"}.ToList();
        secondNames = new string[] {"Idioso", "Ideas", "Ikea", "Izzard", "Ip-Dip", "III", "Imp", "Idiot", "Inky", "Insipid", "I-Smell", "Ice Tea", "Itchy", "Inbred"}.ToList();
        colors = new string[] {"black", "blue", "grey", "orange", "pink", "purple", "red"}.ToList();

        for (int i = 0; i < numberOfEnemies; i++)
        {
            var enemy = Instantiate(EnemyPrefab, Vector2.zero, Quaternion.identity) as GameObject;
            //enemy.GetComponent<Player>().SetName($"AI Player {i + 1}");
            enemy.GetComponent<Player>().SetName(pickRandomName());
            enemy.GetComponent<Player>().SetColor(pickRandomColor());
            enemy.GetComponent<Player>().AIWineLevel = wineLevel;
            enemy.transform.SetParent(enemiesPanel.transform, false);
            players.Add(enemy);
        }
    }

    private List<string> firstNames;
    private List<string> secondNames;

    private string pickRandomName()
    {
        if(firstNames == null || firstNames.Any() == false || secondNames == null || secondNames.Any() == false)
        {
            Debug.Log("Ran out of names!");
            return "AI Name Missing";
        }

        int firstIndex = UnityEngine.Random.Range(0, firstNames.Count);
        string firstName = firstNames[firstIndex];
        firstNames.Remove(firstName);

        int secondIndex = UnityEngine.Random.Range(0, secondNames.Count);
        string secondName = secondNames[secondIndex];
        secondNames.Remove(secondName);

        return $"{firstName} {secondName}";
    }

    private List<string> colors;

    public string pickRandomColor()
    {   
        if(colors == null || colors.Any() == false)
        {
            Debug.Log("Ran out of colors!");
            return "black";
        }
        int randomIndex = UnityEngine.Random.Range(0, colors.Count);

        string color = colors[randomIndex];

        colors.Remove(color);

        return color;
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
        //Debug.Log("clear enemies");

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
