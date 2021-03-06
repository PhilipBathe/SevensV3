﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class SeatManager : NetworkBehaviour
{
    public GameObject EnemyPrefab;

    public GameObject EnemiesPanel;

    public int MinPlayers = 3;
    public int NumberOfAIPlayers = 0;
    public int NumberOfCardPacks = 1;
    public int AIWineLevel = 1;
    public int CountdownSeconds = 10;

    public RoundManager RoundManager;


    //human beings
    private List<GameObject> networkPlayers = new List<GameObject>();

    //mix of humans and AI
    private List<GamePlayer> gamePlayers = new List<GamePlayer>();


    private int seatNumber = 0;
    private bool isGameInProgress = false;

    public void RequestSeat(GameObject networkPlayer)
    {
        networkPlayers.Add(networkPlayer);
        networkPlayer.GetComponent<NetworkPlayer>().SeatNumber = ++seatNumber;

        GamePlayer gamePlayer = new GamePlayer { 
            PlayerName = networkPlayer.GetComponent<NetworkPlayer>().PlayerName,
            SeatNumber = networkPlayer.GetComponent<NetworkPlayer>().SeatNumber,
            NetworkPlayerGO = networkPlayer
        };

        gamePlayers.Add(gamePlayer);

        spawnNewEnemyGameObject(gamePlayer);

        nominateNewTableHost();

        startNewGame();
    }

    public void ChangeNumberOfAIPlayers(int numberOfPlayers)
    {
        if(NumberOfAIPlayers != numberOfPlayers)
        {
            NumberOfAIPlayers = numberOfPlayers;
            killAllAIPlayers();
            createNewAIPlayers();
            startNewGame();
        }
    }

    public void ChangeNumberOfCardPacks(int numberOfPacks)
    {
        Debug.Log("changing number of packs");
        if(NumberOfCardPacks != numberOfPacks)
        {
            NumberOfCardPacks = numberOfPacks;
        }
    }

    public void ChangeWineLevel(int wineLevel)
    {
        AIWineLevel = wineLevel;
        foreach(var player in gamePlayers)
        {
            player.WineLevel = AIWineLevel;
        }
    }

    public void NetworkPlayerDestroyed(GameObject networkPlayer)
    {
        int seatNumber = networkPlayer.GetComponent<NetworkPlayer>().SeatNumber;

        Debug.Log($"Goodbye Number {seatNumber}");

        var leaver = gamePlayers.First(g => g.SeatNumber == seatNumber);

        if(isGameInProgress == true)
        {
            //we turn them into AI so any existing game does not end
            leaver.IsTableHost = false;
            leaver.IsSittingOut = false;
            leaver.IsAI = true;
            leaver.WineLevel = AIWineLevel;
            leaver.EnemyPlayerGO.GetComponent<Enemy>().IsAI = true;
            //leaver.EnemyPlayerGO.GetComponent<Enemy>().PlayerName = $"*{leaver.EnemyPlayerGO.GetComponent<Enemy>().PlayerName}";
            leaver.NetworkPlayerGO = null;
            leaver.ShowPlayerType();

            GameObject.Find("AIManager").GetComponent<AIManager>().NumberOfAIPlayers = ++NumberOfAIPlayers;

            RoundManager.NetworkPlayerNowAI(seatNumber);
        }
        else
        {
            gamePlayers.Remove(leaver);
            leaver.EnemyPlayerGO.transform.SetParent(null);
            Destroy(leaver.EnemyPlayerGO);
        }

        networkPlayers.Remove(networkPlayer);

        nominateNewTableHost();
    }

    private void nominateNewTableHost()
    {
        //Debug.Log("nominateNewTableHost");
        if(gamePlayers.Count(p => p.IsTableHost == true) == 0)
        {
            //Debug.Log("need a new host");
            var firstHuman = gamePlayers.OrderBy(p => p.IsSittingOut).ThenBy(p => p.SeatNumber).FirstOrDefault(p => p.IsAI == false);
            if(firstHuman != null)
            {
                //Debug.Log("found a new host");
                firstHuman.IsTableHost = true;
                firstHuman.ShowIsTableHost();
            }
        }
    }

    public void ToggleSitOut(GameObject networkPlayer)
    {
        bool wasAlreadyLegalToPlay = isLegalToPlay();
        int seatNumber = networkPlayer.GetComponent<NetworkPlayer>().SeatNumber;

        Debug.Log($"Number {seatNumber} is toggling sit out");

        var player = gamePlayers.First(g => g.SeatNumber == seatNumber);
        player.IsSittingOut = !player.IsSittingOut;

        bool isPlayerInCurrentGame = false;

        if(isGameInProgress == true)
        {
            RoundManager.NetworkPlayerToggleSittingOut(seatNumber, player.IsSittingOut, out isPlayerInCurrentGame);
            if(isPlayerInCurrentGame == false)
            {
                player.ToggleBetweenGameSittingOutStatus();
            }
        }
        else
        {
            player.ToggleBetweenGameSittingOutStatus();
            if(wasAlreadyLegalToPlay == false && isLegalToPlay() == true)
            {
                startNewGame();
            }
        }
    }

    private void killAllAIPlayers()
    {
        gamePlayers.RemoveAll(p => p.IsAI == true);
        List<GameObject> buggersToKill = new List<GameObject>();

        foreach(GameObject child in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            if(child.GetComponent<Enemy>().IsAI == true)
            {
                buggersToKill.Add(child);
            }
        }

        foreach(var child in buggersToKill)
        {
            child.GetComponent<Enemy>().RpcDie();
            Destroy(child);
        }
    }

    private void createNewAIPlayers()
    {
        for(int i = 0; i < NumberOfAIPlayers; i++)
        {
            addAIPlayer();
        }
    }

    private void addAIPlayer()
    {
        GamePlayer gamePlayer = new GamePlayer { 
            PlayerName = pickRandomName(),
            SeatNumber = ++seatNumber,
            IsAI = true,
            WineLevel = AIWineLevel
        };

        spawnNewEnemyGameObject(gamePlayer);

        gamePlayers.Add(gamePlayer);
    }

    private void startNewGame()
    {
        if(isGameInProgress == true)
        {
            return;
        }

        if(countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
        }

        if(isLegalToPlay() == false)
        {
            showWaitingForPlayers();
            return;
        }

        countdownCoroutine = StartCoroutine(countdown());
    }

    private Coroutine countdownCoroutine;

    IEnumerator countdown()
    {
        startClientCountdown(CountdownSeconds);
        yield return new WaitForSeconds(CountdownSeconds);

        //check if players have sat out or left
        if(isLegalToPlay() == false)
        {
            showWaitingForPlayers();
            yield break;
        }

        isGameInProgress = true;
        hideNextGamePanel();
        setSitOutStatuses();
        //Debug.Log($"new game: {NumberOfCardPacks}");
        RoundManager.StartNewGame(gamePlayers.Where(p => p.IsSittingOut == false).ToList(), NumberOfCardPacks);  
    }

    private void setSitOutStatuses()
    {
        foreach(var player in gamePlayers.Where(p => p.IsSittingOut == true))
        {
            player.Reset("Sitting Out");
        }
    }

    private void showWaitingForPlayers()
    {
        foreach(GameObject networkPlayer in networkPlayers)
        {
            networkPlayer.GetComponent<NetworkPlayer>().RpcShowWaitingForPlayers();
        }
    }

    private void startClientCountdown(int countdownSeconds)
    {
        foreach(GameObject networkPlayer in networkPlayers)
        {
            networkPlayer.GetComponent<NetworkPlayer>().RpcStartCountdown(countdownSeconds);
        }      
    }

    private void hideNextGamePanel()
    {
        foreach(GameObject networkPlayer in networkPlayers)
        {
            networkPlayer.GetComponent<NetworkPlayer>().RpcHideNextGamePanel();
        }      
    }

    public void GameFinished()
    {
        isGameInProgress = false;

        startNewGame();
    }

    private bool isLegalToPlay()
    {
        return gamePlayers.Count(p => p.IsSittingOut == false) >= MinPlayers;
    }

    private void spawnNewEnemyGameObject(GamePlayer gamePlayer)
    {
        var go = Instantiate(EnemyPrefab, Vector2.zero, Quaternion.identity);

        NetworkServer.Spawn(go);

        var enemy = go.GetComponent<Enemy>();

        enemy.PlayerName = gamePlayer.PlayerName;
        enemy.SeatNumber = gamePlayer.SeatNumber;
        enemy.PlayerColor = pickRandomColor();
        enemy.IsAI = gamePlayer.IsAI;
        enemy.Parent = EnemiesPanel;
        enemy.StatusText = "Waiting for next game";

        gamePlayer.EnemyPlayerGO = go;

        StartCoroutine(HackyCoroutine());
    }

    //Waiting for last gameobject to get its transform synced up - there is probably a better way!
    private IEnumerator HackyCoroutine()
    {
        yield return new WaitForSeconds(1);
        AnnounceNewEnemy();
    }

    private void AnnounceNewEnemy()
    {
        foreach(var player in gamePlayers)
        {
            player.SortEnemies();
        }
    }

    private List<string> colors;

    private string pickRandomColor()
    {   
        if(colors == null || colors.Any() == false)
        {
            colors = new string[] {"black", "blue", "grey", "orange", "pink", "purple", "red"}.ToList();
        }

        int randomIndex = UnityEngine.Random.Range(0, colors.Count);

        string color = colors[randomIndex];

        colors.Remove(color);

        return color;
    }

    private List<string> firstNames;
    private List<string> secondNames;

    private string pickRandomName()
    {
        if(firstNames == null || firstNames.Any() == false) //secondNames == null || secondNames.Any() == false)
        {
            firstNames = new string[] {"Alberto", "Ali", "Andrew", "Alice", "Art", "Ant", "Amy", "Alesha", "Anjie", "Archie", "Arry", "Alex", "Angel", "Axl"}.ToList();
            //Debug.Log("Ran out of first names!");
        }

        if(secondNames == null || secondNames.Any() == false)
        {
            secondNames = new string[] {"Idioso", "Ideas", "Ikea", "Izzard", "Ip-Dip", "III", "Imp", "Idiot", "Inky", "Insipid", "I-Smell", "Ice Tea", "Itchy", "Inbred"}.ToList();
            //Debug.Log("Ran out of second names!");
        }

        int firstIndex = UnityEngine.Random.Range(0, firstNames.Count);
        string firstName = firstNames[firstIndex];
        firstNames.Remove(firstName);

        int secondIndex = UnityEngine.Random.Range(0, secondNames.Count);
        string secondName = secondNames[secondIndex];
        secondNames.Remove(secondName);

        return $"{firstName} {secondName}";
    }
}
