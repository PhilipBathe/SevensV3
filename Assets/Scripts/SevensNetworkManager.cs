using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class SevensNetworkManager : NetworkManager
{
    public List<GameObject> SpawnPrefabs 
    { 
        get { return spawnPrefabs; }
        set { spawnPrefabs = value; } 
    }

    public SeatManager SeatManager;

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        var networkPlayerIdentity = conn.clientOwnedObjects.FirstOrDefault(o => o.gameObject.GetComponent<NetworkPlayer>() != null);

        if(networkPlayerIdentity != null)
        {
            SeatManager.NetworkPlayerDestroyed(networkPlayerIdentity.gameObject);
        }
        else
        {
            Debug.Log("couldn't find the game object for the destroying player");
        }

        base.OnServerDisconnect(conn);
    }

    public override void OnStartServer()
    {
        //GameserverSDK.Start();
        base.OnStartServer();
    }

    //TODO: handle when a LAN host stops
    // OnStopHost
    // OnStopServer (also when a LAN server only stops)
    // OnStopClient
}
