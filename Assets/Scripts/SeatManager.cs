using System.Collections;
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

        spawnNewGameObject(gamePlayer);

        gamePlayers.Add(gamePlayer);

        startNewGame();

        //TODO: deal with people leaving the table affecting seatnumber (and remove them from enemies panel)
    }

    public void ChangeNumberOfAIPlayers(int numberOfPlayers)
    {
        int newNumber = numberOfPlayers;

        if(NumberOfAIPlayers != newNumber)
        {
            NumberOfAIPlayers = newNumber;
            killAllAIPlayers();
            createNewAIPlayers();
            startNewGame();
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
        //Debug.Log($"createNewAIPlayers {NumberOfAIPlayers}");
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
            IsAI = true
        };

        spawnNewGameObject(gamePlayer);

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
        RoundManager.StartNewGame(gamePlayers);
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
        if(gamePlayers.Count < MinPlayers)
        {
            return false;
        }

        // if(isGameInProgress == true)
        // {
        //     return false;
        // }

        return true;
    }

    private void spawnNewGameObject(GamePlayer gamePlayer)
    {
        var go = Instantiate(EnemyPrefab, Vector2.zero, Quaternion.identity);

        NetworkServer.Spawn(go);

        var enemy = go.GetComponent<Enemy>();

        enemy.PlayerName = gamePlayer.PlayerName;
        enemy.SeatNumber = gamePlayer.SeatNumber;
        enemy.PlayerColor = pickRandomColor();
        enemy.IsAI = gamePlayer.IsAI;
        enemy.Parent = EnemiesPanel;

        gamePlayer.EnemyPlayerGO = go;
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
            Debug.Log("Ran out of first names!");
        }

        if(secondNames == null || secondNames.Any() == false)
        {
            secondNames = new string[] {"Idioso", "Ideas", "Ikea", "Izzard", "Ip-Dip", "III", "Imp", "Idiot", "Inky", "Insipid", "I-Smell", "Ice Tea", "Itchy", "Inbred"}.ToList();
            Debug.Log("Ran out of second names!");
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
