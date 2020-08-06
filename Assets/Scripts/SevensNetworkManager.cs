using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using System;

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
        #if UNITY_SERVER
			clearOfflineScene();
        #endif

        base.OnStartServer();
    }

    private void clearOfflineScene()
    {
        this.offlineScene = string.Empty;
    }
}
