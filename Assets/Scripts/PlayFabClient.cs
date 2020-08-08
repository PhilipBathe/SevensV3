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
		//Debug.Log(response.ToString());

        PlayFab.ClientModels.EntityKey clientEntityKey = response.EntityToken.Entity;

        //TODO: show waiting for players in UI

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

        GetMatchmakingTicketRequest request = new GetMatchmakingTicketRequest{
            TicketId = result.TicketId,
            QueueName = "FamilyGame",
            EscapeObject = false
        };

        //poll GetMatchmakingTicket to get match id
        ticketPoller = StartCoroutine(checkForMatchId(request));
    }

    Coroutine ticketPoller;

    private IEnumerator checkForMatchId(GetMatchmakingTicketRequest request)
    {
        while (true)
        {
            Debug.Log("polling");
            //can poll up to 10 times a minute
            yield return new WaitForSeconds(10);
            PlayFabMultiplayerAPI.GetMatchmakingTicket(request, onGetMatchmakingTicketSuccess, onGetMatchmakingTicketError);
        }
    }

    private void onGetMatchmakingTicketError(PlayFabError response)
    {
        //TODO: show the problem in UI
		Debug.Log(response.ToString());
    }

    private void onGetMatchmakingTicketSuccess(GetMatchmakingTicketResult result)
    {
        Debug.Log("onGetMatchmakingTicketSuccess");

        Debug.Log($"result.MatchId: {result.MatchId}");

        if(string.IsNullOrEmpty(result.MatchId) == false)
        {
            Debug.Log("onGetMatchmakingTicketSuccess - has match ID");
            StopCoroutine(ticketPoller);

            //use GetMatch to get IP and port
            GetMatchRequest request = new GetMatchRequest {
                EscapeObject = false,
                QueueName = "FamilyGame",
                ReturnMemberAttributes = false,
                MatchId = result.MatchId
            };
            PlayFabMultiplayerAPI.GetMatch(request, onGetMatchSuccess, onGetMatchError);
        }
    }

    private void onGetMatchError(PlayFabError response)
    {
        //TODO: show the problem in UI
		Debug.Log(response.ToString());
    }

    private void onGetMatchSuccess(GetMatchResult result)
    {
        Debug.Log("onGetMatchSuccess");
        Debug.Log($"result.ServerDetails.IPV4Address, {result.ServerDetails.IPV4Address}");
        Debug.Log($"result.ServerDetails.Ports[0].Num, {result.ServerDetails.Ports[0].Num}");

        configuration.ipAddress = result.ServerDetails.IPV4Address;
		configuration.port = (ushort)result.ServerDetails.Ports[0].Num;
        StartCoroutine(ConnectRemoteClient());
    }

	private IEnumerator ConnectRemoteClient()
	{
        //let server get ready - or can we check some other way?
        yield return new WaitForSeconds(10);
        networkManager.networkAddress = configuration.ipAddress;
        telepathyTransport.port = configuration.port;
        //apathyTransport.port = configuration.port;

		networkManager.StartClient();
	}


    public void HardCodedJoin()
    {
        networkManager = this.GetComponent<SevensNetworkManager>();
        telepathyTransport = this.GetComponent<TelepathyTransport>();
        
        networkManager.networkAddress = "52.224.151.249";
        telepathyTransport.port = 30001;
        //apathyTransport.port = configuration.port;

		networkManager.StartClient();
    }








	// private void OnRequestServerDetails(GetMultiplayerServerDetailsResponse response)
	// {
	// 	Debug.Log("OnRequestServerDetails");

	// 	if(response != null)
	// 	{
	// 		Debug.Log($"response.IPV4Address {response.IPV4Address}");
	// 		configuration.ipAddress = response.IPV4Address;
	// 		configuration.port = (ushort)response.Ports[0].Num;
	// 		ConnectRemoteClient();
	// 	}
	// 	else
	// 	{
	// 		RequestMultiplayerServer(); 
	// 	}
	// }

	// private void OnRequestServerDetailsError(PlayFabError error)
	// {
	// 	Debug.Log($"OnRequestServerDetailsError");
	// 	Debug.Log(error.ErrorMessage);

	// 	if(error.ErrorDetails != null)
	// 	{
	// 		foreach(var errDic in error.ErrorDetails)
	// 		{
	// 			Debug.Log($"key --- {errDic.Key}");
	// 			foreach(var val in errDic.Value)
	// 			{
	// 				Debug.Log(val);
	// 			}
	// 		}
	// 	}
	// }


    // private void RequestMultiplayerServer()
	// {
	// 	Debug.Log("[ClientStartUp].RequestMultiplayerServer");
	// 	Debug.Log($"configuration.ipAddress {configuration.ipAddress}");
	// 	RequestMultiplayerServerRequest requestData = new RequestMultiplayerServerRequest();
	// 	requestData.BuildId = configuration.buildId;
	// 	requestData.SessionId = "5c48a303-e25b-4afc-8c8e-d5c1d459c850"; //System.Guid.NewGuid().ToString();
	// 	requestData.PreferredRegions = new List<string>() { "WestUs" };
	// 	PlayFabMultiplayerAPI.RequestMultiplayerServer(requestData, OnRequestMultiplayerServer, OnRequestMultiplayerServerError);
	// }

	// private void OnRequestMultiplayerServer(RequestMultiplayerServerResponse response)
	// {
	// 	Debug.Log($"OnRequestMultiplayerServer {response}");
	// 	ConnectRemoteClient(response);
	// }



	// private void OnRequestMultiplayerServerError(PlayFabError error)
	// {
	// 	Debug.Log($"OnRequestMultiplayerServerError");
	// 	Debug.Log(error.ErrorMessage);

	// 	if(error.ErrorDetails != null)
	// 	{
	// 		foreach(var errDic in error.ErrorDetails)
	// 		{
	// 			Debug.Log($"key --- {errDic.Key}");
	// 			foreach(var val in errDic.Value)
	// 			{
	// 				Debug.Log(val);
	// 			}
	// 		}
	// 	}
	// }
}
