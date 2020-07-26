using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class NetworkPlayer : NetworkBehaviour
{
    [SyncVar]
    public string PlayerName;

    [SyncVar]
    public int SeatNumber;

    public GameObject PlayerPrefab;


    // This fires on server when this player object is network-ready
    public override void OnStartServer()
    {
        base.OnStartServer();

        //playerNo = connectionToClient.connectionId;
        
    }

    // This only fires on the local client when this player object is network-ready
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        var name = PlayerPrefs.GetString("username").Trim();

        if(name.Length == 0)
        {
            name = "Mystery Unicorn";
        }
        if(name.Length > 15)
        {
            name = name.Substring(0, 15);
        }

        CmdSetPlayerName(name);
        setupUI();
    }

    [Command]
    void CmdSetPlayerName(string name)
    {
        Debug.Log($"CmdSetPlayerName {name}");
        PlayerName = name;
        GameObject.Find("SeatManager").GetComponent<SeatManager>().RequestSeat(gameObject);
    }

    private void setupUI()
    {
        var canvas = GameObject.Find("Canvas");

        var player = Instantiate(PlayerPrefab, Vector2.zero, Quaternion.identity) as GameObject;
        player.transform.SetParent(canvas.transform, false);
        player.GetComponent<Player>().SetStatus("Waiting for next game");
    }

}
