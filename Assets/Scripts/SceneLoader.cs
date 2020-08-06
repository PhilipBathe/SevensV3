using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using PlayFab;
using PlayFab.ClientModels;
using System;
using PlayFab.MultiplayerModels;

public class SceneLoader : NetworkBehaviour
{
    private  Configuration configuration;
    private SevensNetworkManager networkManager;
    private TelepathyTransport telepathyTransport;

    void Start()
    {
        networkManager = this.GetComponent<SevensNetworkManager>();
        telepathyTransport = this.GetComponent<TelepathyTransport>();
        configuration = this.GetComponent<Configuration>();

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
            case "JoinOnline":
                joinOnline();
                break;
            case "SinglePlayer":
            default:
                networkManager.StartHost();
                //TODO: prevent others from joining
                break;
        }
    }

    private void joinOnline()
    {
        Debug.Log("joinOnline");

        //We need to login a user to get at PlayFab API's. 
		LoginWithCustomIDRequest request = new LoginWithCustomIDRequest()
		{
			TitleId = PlayFabSettings.TitleId,
			CreateAccount = true,
			CustomId = Guid.NewGuid().ToString()
		};

		PlayFabClientAPI.LoginWithCustomID(request, OnPlayFabLoginSuccess, OnLoginError);
    }

    private void OnLoginError(PlayFabError response)
	{
		Debug.Log(response.ToString());
	}

    private void OnPlayFabLoginSuccess(LoginResult response)
	{
		Debug.Log(response.ToString());
		if (configuration.ipAddress == "")
		{   //We need to grab an IP and Port from a server based on the buildId. Copy this and add it to your Configuration.
			RequestMultiplayerServer(); 
		}
		else
		{
			ConnectRemoteClient();
		}
	}

    private void RequestMultiplayerServer()
	{
		Debug.Log("[ClientStartUp].RequestMultiplayerServer");
		Debug.Log($"configuration.ipAddress {configuration.ipAddress}");
		RequestMultiplayerServerRequest requestData = new RequestMultiplayerServerRequest();
		requestData.BuildId = configuration.buildId;
		requestData.SessionId = System.Guid.NewGuid().ToString();
		requestData.PreferredRegions = new List<string>() { "WestUs" };
		PlayFabMultiplayerAPI.RequestMultiplayerServer(requestData, OnRequestMultiplayerServer, OnRequestMultiplayerServerError);
	}

	private void OnRequestMultiplayerServer(RequestMultiplayerServerResponse response)
	{
		Debug.Log($"OnRequestMultiplayerServer {response}");
		ConnectRemoteClient(response);
	}

	private void ConnectRemoteClient(RequestMultiplayerServerResponse response = null)
	{
		if(response == null) 
		{
			networkManager.networkAddress = configuration.ipAddress;
			telepathyTransport.port = configuration.port;
			//apathyTransport.port = configuration.port;
		}
		else
		{
			Debug.Log("**** ADD THIS TO YOUR CONFIGURATION **** -- IP: " + response.IPV4Address + " Port: " + (ushort)response.Ports[0].Num);
			networkManager.networkAddress = response.IPV4Address;
			telepathyTransport.port = (ushort)response.Ports[0].Num;
			//apathyTransport.port = (ushort)response.Ports[0].Num;
		}

		networkManager.StartClient();
	}

	private void OnRequestMultiplayerServerError(PlayFabError error)
	{
		Debug.Log($"OnRequestMultiplayerServerError");
		Debug.Log(error.ErrorMessage);

		if(error.ErrorDetails != null)
		{
			foreach(var errDic in error.ErrorDetails)
			{
				Debug.Log($"key --- {errDic.Key}");
				foreach(var val in errDic.Value)
				{
					Debug.Log(val);
				}
			}
		}
	}
}
