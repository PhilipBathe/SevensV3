using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SceneLoader : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var networkManager = this.GetComponent<SevensNetworkManager>();

        var entranceChoice = PlayerPrefs.GetString("entranceChoice").Trim();

        switch(entranceChoice)
        {
            case "HostLan":
            case "SinglePlayer":
            default:
                networkManager.StartHost();
                break;
            case "LanServer":
                networkManager.StartServer();
                break;
            case "JoinLan":
                //TODO: handle invalid LAN addressese here
                networkManager.networkAddress = PlayerPrefs.GetString("ipAddress").Trim();
                networkManager.StartClient();
                break;
        }
    }

    //TODO: handle when people leave
}
