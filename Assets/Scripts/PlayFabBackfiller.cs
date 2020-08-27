using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.MultiplayerModels;
using Mirror;

public class PlayFabBackfiller : NetworkBehaviour
{
    private List<MatchmakingPlayerWithTeamAssignment> members;
    private ServerDetails serverDetails;

    public void SetRecentPlayerDetails(string matchResultJson)
    {
		GetMatchResult getMatchResult = JsonUtility.FromJson<GetMatchResult>(matchResultJson);
        members = getMatchResult.Members;
        serverDetails = getMatchResult.ServerDetails;

		if(members.Count < 12) //TODO: move this magic number into a configuration somewhere (when we can link it to the queue)
		{
			lookForMorePlayers();
		}
    }

	private void lookForMorePlayers()
	{
		Debug.Log("look for more players");

		if(getPlayersCoroutine != null)
		{
			StopCoroutine(getPlayersCoroutine);
		}

		getPlayersCoroutine = StartCoroutine(getMorePlayers());
	}

	Coroutine getPlayersCoroutine;

    private IEnumerator getMorePlayers()
	{
		yield return new WaitForSeconds(10f);

		// while (true)
        // {
			CreateServerBackfillTicketRequest request = new CreateServerBackfillTicketRequest {
				GiveUpAfterSeconds = 60,
				QueueName = "FamilyGame",
				Members = members,
				ServerDetails =  serverDetails
			}; 
			PlayFabMultiplayerAPI.CreateServerBackfillTicket(request, onCreateServerBackfillTicketSuccess, onCreateServerBackfillTicketError);
			yield return new WaitForSeconds(70f);
		//}
		
	}

	private void onCreateServerBackfillTicketSuccess(CreateServerBackfillTicketResult result)
	{
		var playFabMessageRelay = GameObject.Find("PlayFabMessageRelay").GetComponent<PlayFabMessageRelay>();
		playFabMessageRelay.RpcSendClientMessage("onCreateServerBackfillTicketSuccess");
		//TODO: do we need anything here?
	}

	private void onCreateServerBackfillTicketError(PlayFabError error)
	{
		var playFabMessageRelay = GameObject.Find("PlayFabMessageRelay").GetComponent<PlayFabMessageRelay>();
		playFabMessageRelay.RpcSendClientMessage(error.ErrorMessage);
	}
}
