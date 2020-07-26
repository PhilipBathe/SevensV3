using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class SeatManager : NetworkBehaviour
{
    public GameObject EnemyPrefab;

    public GameObject EnemiesPanel;

    public int MinPlayers = 1;

    public Animator NewGamePanel;

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

        checkIfWeCanPlay();

        //TODO: deal with people leaving the table affecting seatnumber (and remove them from enemies panel)
    }

    public void StartNewGame()
    {
        hideNextGamePanel();
        isGameInProgress = true;
        RoundManager.StartNewGame(gamePlayers);
    }

    public void GameFinished()
    {
        isGameInProgress = false;
        checkIfWeCanPlay();
    }


    private void checkIfWeCanPlay()
    {
        //gamePlayers.Count >= MinPlayers && 
        if(isGameInProgress == false)
        {
            //show new game options to first player

            //TODO: trigger this in one player's UI (currently running on host)
            showNextGamePanel();
        }
    }

    private void spawnNewGameObject(GamePlayer gamePlayer)
    {
        var go = Instantiate(EnemyPrefab, Vector2.zero, Quaternion.identity);

        NetworkServer.Spawn(go);

        var enemy = go.GetComponent<Enemy>();

        enemy.PlayerName = gamePlayer.PlayerName;
        enemy.SeatNumber = gamePlayer.SeatNumber;
        enemy.PlayerColor = pickRandomColor();
        enemy.Parent = EnemiesPanel;
        enemy.Status = "Waiting for next game";

        gamePlayer.GameObject = go;

        //enemy.GetComponent<AIPlayer>().AIWineLevel = wineLevel;
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

    private void hideNextGamePanel()
    {
        NewGamePanel.SetBool("isHidden", true);
    }

    private void showNextGamePanel()
    {
        NewGamePanel.SetBool("isHidden", false);
    }
}
