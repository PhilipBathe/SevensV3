using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SceneLoader : NetworkBehaviour
{
    void Start()
    {
        var networkManager = this.GetComponent<SevensNetworkManager>();

        #if UNITY_SERVER
            //mirror server does this automagically if running as a headless server mode thingie
            //networkManager.StartServer();
            return;
        #endif

        var entranceChoice = PlayerPrefs.GetString("entranceChoice").Trim();

        switch(entranceChoice)
        {
            case "HostLan":
                networkManager.StartHost();
                break;
            case "LanServer":
                networkManager.StartServer();
                //TODO: show number of humans playing etc.
                //Also hide board
                break;
            case "JoinLan":
                //TODO: handle invalid LAN addressese here
                networkManager.networkAddress = PlayerPrefs.GetString("ipAddress").Trim();
                networkManager.StartClient();
                break;
            case "SinglePlayer":
            default:
                networkManager.StartHost();
                //TODO: prevent others from joining
                break;
        }
    }
}
