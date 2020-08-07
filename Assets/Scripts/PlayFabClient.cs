using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using PlayFab;
using PlayFab.ClientModels;
using System;
using PlayFab.MultiplayerModels;

public class PlayFabClient : NetworkBehaviour
{
    private  Configuration configuration;
    private SevensNetworkManager networkManager;
    private TelepathyTransport telepathyTransport;


    void Start()
    {
        networkManager = this.GetComponent<SevensNetworkManager>();
        telepathyTransport = this.GetComponent<TelepathyTransport>();
        configuration = this.GetComponent<Configuration>();
    }

    public void JoinOnline()
    {
        Debug.Log("joinOnline");

        var customId = getCustomId();

		LoginWithCustomIDRequest request = new LoginWithCustomIDRequest()
		{
			TitleId = PlayFabSettings.TitleId,
			CreateAccount = true,
			CustomId = customId
		};

		PlayFabClientAPI.LoginWithCustomID(request, onPlayFabLoginSuccess, onLoginError);
    }

    private string getCustomId()
    {
        var customId = PlayerPrefs.GetString("customId").Trim();
        if(string.IsNullOrEmpty(customId))
        {
            customId = Guid.NewGuid().ToString();
            PlayerPrefs.SetString("customId", customId);
        }

        return customId;
    }

    private void onLoginError(PlayFabError response)
	{
        //TODO: show the problem in UI
		Debug.Log(response.ToString());
	}

    private void onPlayFabLoginSuccess(LoginResult response)
	{
		Debug.Log(response.ToString());

        PlayFab.ClientModels.EntityKey clientEntityKey = response.EntityToken.Entity;

        //TODO: show waiting for players in UI

        //join matchmaking queue or get back fill thingie?
        CreateMatchmakingTicketRequest request = new CreateMatchmakingTicketRequest();
        request.QueueName = "FamilyGame";
        request.GiveUpAfterSeconds = 60; //TODO: probably need to increase this knowing my family!
        request.Creator = new MatchmakingPlayer{
            Entity = new PlayFab.MultiplayerModels.EntityKey { Id = clientEntityKey.Id, Type = clientEntityKey.Type },
            Attributes = new MatchmakingPlayerAttributes{ EscapedDataObject = "{ \"mu\": 16.0, \"sigma\": 1.8, \"latencies\": [ { \"region\": \"EastUs\", \"latency\": 150 }, { \"region\": \"WestUs\", \"latency\": 400 } ] }" }
        };

        PlayFabMultiplayerAPI.CreateMatchmakingTicket(request, onMatchmakingTicketCreateSuccess, onMatchmakingTicketCreateError);
	}

    private void onMatchmakingTicketCreateError(PlayFabError response)
    {
        //TODO: show the problem in UI
		Debug.Log(response.ToString());
    }

    private void onMatchmakingTicketCreateSuccess(CreateMatchmakingTicketResult result)
    {
        Debug.Log("onMatchmakingTicketCreateSuccess called");
    }


	private void OnRequestServerDetails(GetMultiplayerServerDetailsResponse response)
	{
		Debug.Log("OnRequestServerDetails");

		if(response != null)
		{
			Debug.Log($"response.IPV4Address {response.IPV4Address}");
			configuration.ipAddress = response.IPV4Address;
			configuration.port = (ushort)response.Ports[0].Num;
			ConnectRemoteClient();
		}
		else
		{
			RequestMultiplayerServer(); 
		}
	}

	private void OnRequestServerDetailsError(PlayFabError error)
	{
		Debug.Log($"OnRequestServerDetailsError");
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


    private void RequestMultiplayerServer()
	{
		Debug.Log("[ClientStartUp].RequestMultiplayerServer");
		Debug.Log($"configuration.ipAddress {configuration.ipAddress}");
		RequestMultiplayerServerRequest requestData = new RequestMultiplayerServerRequest();
		requestData.BuildId = configuration.buildId;
		requestData.SessionId = "5c48a303-e25b-4afc-8c8e-d5c1d459c850"; //System.Guid.NewGuid().ToString();
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
