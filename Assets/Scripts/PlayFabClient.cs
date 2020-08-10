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

    private MessageManager messageManager;


    void Start()
    {
        networkManager = this.GetComponent<SevensNetworkManager>();
        telepathyTransport = this.GetComponent<TelepathyTransport>();
        configuration = this.GetComponent<Configuration>();
        messageManager = GameObject.Find("MessageManager").GetComponent<MessageManager>();
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
        handleError(response);
	}

    private void onPlayFabLoginSuccess(LoginResult response)
	{
		//Debug.Log(response.ToString());

        PlayFab.ClientModels.EntityKey clientEntityKey = response.EntityToken.Entity;

        messageManager.ShowNewMessage("Who is up for sevens?");

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
        handleError(response);
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
            messageManager.AddMessage("Anyone want to play?");
        }
    }

    private void onGetMatchmakingTicketError(PlayFabError response)
    {
        handleError(response);
    }

    private void onGetMatchmakingTicketSuccess(GetMatchmakingTicketResult result)
    {
        Debug.Log("onGetMatchmakingTicketSuccess");

        Debug.Log($"result.MatchId: {result.MatchId}");

        if(string.IsNullOrEmpty(result.MatchId) == false)
        {
            messageManager.AddMessage("Yay! I'll wipe the table - you get the cards");
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
        handleError(response);
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
        messageManager.AddMessage("Actually, I just need the loo, back in a sec");
        yield return new WaitForSeconds(10);
        networkManager.networkAddress = configuration.ipAddress;
        telepathyTransport.port = configuration.port;

        messageManager.HideMessagePanel();
		networkManager.StartClient();
	}

    private void handleError(PlayFabError error)
    {
        //TODO: actually handle this!

        Debug.Log(error.ErrorMessage);

        messageManager.AddMessage("Doh! Something has gone wrong :-(");
        messageManager.AddMessage("Can someone tell Philip please?  Meanwhile - maybe just try closing the app down completely and then starting again.");

        messageManager.AddMessage(error.ErrorMessage);

		if(error.ErrorDetails != null)
		{
			foreach(var errDic in error.ErrorDetails)
			{
				messageManager.AddMessage($"key --- {errDic.Key}");
				foreach(var val in errDic.Value)
				{
					messageManager.AddMessage(val);
				}
			}
		}
    }
}
